using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using Mono.Collections.Generic;
using Tracer.Fody.Helpers;

namespace Tracer.Fody.Weavers
{
    /// <summary>
    /// Executes weaving on the given type.
    /// </summary>
    internal class TypeWeaver
    {
        private readonly TypeDefinition _typeDefinition;
        private readonly ITraceLoggingFilter _filter;
        private readonly TypeReferenceProvider _typeReferenceProvider;
        private readonly MethodReferenceProvider _methodReferenceProvider;
        private readonly Lazy<FieldDefinition> _staticLoggerField;

        internal TypeWeaver(ITraceLoggingFilter filter, TypeReferenceProvider typeReferenceProvider, MethodReferenceProvider methodReferenceProvider,
            TypeDefinition typeDefinition)
        {
            _filter = filter;
            _typeReferenceProvider = typeReferenceProvider;
            _methodReferenceProvider = methodReferenceProvider;
            _typeDefinition = typeDefinition;
            _staticLoggerField = new Lazy<FieldDefinition>(CreateLoggerStaticField);
        }

        public void Execute()
        {
            foreach (var method in _typeDefinition.GetMethods().Where(method => method.HasBody && !method.IsAbstract).ToList())
            {
                bool shouldAddTrace = _filter.ShouldAddTrace(method);
                var body = method.Body;
                body.SimplifyMacros();

                if (shouldAddTrace)
                {
                    bool hasLoggableParams = body.Method.Parameters.Count(param => !param.IsOut) > 0;
                    if (hasLoggableParams) { WeaveTraceEnterWithParameters(body); }
                    else { WeaveTraceEnterWithoutParameters(body); }

                    bool hasReturnValue = (method.ReturnType.MetadataType != MetadataType.Void);
                    if (hasReturnValue) { WeaveTraceLeaveWithReturnValue(body) ;}
                    else { WeaveTraceLeaveWithoutReturnValue(body); }
                }

                SearchForAndChangeStaticLogCalls(body);

                body.OptimizeMacros();
            }
        }

        private void WeaveTraceLeaveWithReturnValue(MethodBody body)
        {
            //----------------------

            /* TRACE Leave: 
               * What we'd like to achieve is:
               * ...(existing code)... 
               *  long methodTimeInTicks = Stopwatch.GetTimestamp() - startTick;
               * _log.TraceLeave("MethodFullName", methodTimeInTicks, returnValue)
               * 
            */

            //Declare local variable for the return value
            var returnType = body.Method.ReturnType;
            var returnValueDef = new VariableDefinition("returnValue", returnType);
            body.Variables.Add(returnValueDef);
            var startTickVar = body.Variables.First(var => var.Name.Equals("startTick"));
            
            var allReturns = body.Instructions.Where(instr => instr.OpCode == OpCodes.Ret).ToList();

            foreach (var @return in allReturns)
            {
                var instructions = new List<Instruction>();

                instructions.Add(Instruction.Create(OpCodes.Stloc, returnValueDef)); //store it in local variable

                var logTraceLeaveMethod = MethodReferenceProvider.GetTraceLeaveWithReturnValueReference();
                instructions.Add(Instruction.Create(OpCodes.Ldsfld, StaticLogger));
                instructions.Add(Instruction.Create(OpCodes.Ldstr, body.Method.FullName));

                //calculate ticks elapsed
                var getTimestampMethod = MethodReferenceProvider.GetTimestampReference();
                instructions.Add(Instruction.Create(OpCodes.Call, getTimestampMethod));
                instructions.Add(Instruction.Create(OpCodes.Ldloc, startTickVar));
                instructions.Add(Instruction.Create(OpCodes.Sub));

                //get return value
                instructions.Add(Instruction.Create(OpCodes.Ldloc, returnValueDef));

                //boxing if needed
                if (returnType.IsPrimitive || returnType.IsGenericParameter)
                {
                    instructions.Add(Instruction.Create(OpCodes.Box, returnType));
                }

                instructions.Add(Instruction.Create(OpCodes.Callvirt, logTraceLeaveMethod));
                instructions.Add(Instruction.Create(OpCodes.Ldloc, returnValueDef)); //read from local variable

                instructions.Add(Instruction.Create(OpCodes.Ret));

                body.Replace(@return, instructions);
            }
        }

        private void WeaveTraceLeaveWithoutReturnValue(MethodBody body)
        {
            //----------------------
            /* TRACE Leave: 
               * What we'd like to achieve is:
               * ...(existing code)... 
               *  long methodTimeInTicks = Stopwatch.GetTimestamp() - startTick;
               * _log.TraceLeave("MethodFullName", methodTimeInTicks)
               * 
            */
            var allReturns = body.Instructions.Where(instr => instr.OpCode == OpCodes.Ret).ToList();

            var startTickVar = body.Variables.First(var => var.Name.Equals("startTick"));

            foreach (var @return in allReturns)
            {
                var instructions = new List<Instruction>();

                var logTraceLeaveMethod =
                    MethodReferenceProvider.GetTraceLeaveWithoutReturnValueReference();

                instructions.Add(Instruction.Create(OpCodes.Ldsfld, StaticLogger));
                instructions.Add(Instruction.Create(OpCodes.Ldstr, body.Method.FullName));

                //calculate ticks elapsed
                var getTimestampMethod = MethodReferenceProvider.GetTimestampReference();
                instructions.Add(Instruction.Create(OpCodes.Call, getTimestampMethod));
                instructions.Add(Instruction.Create(OpCodes.Ldloc, startTickVar));
                instructions.Add(Instruction.Create(OpCodes.Sub));

                //call log
                instructions.Add(Instruction.Create(OpCodes.Callvirt, logTraceLeaveMethod));

                instructions.Add(Instruction.Create(OpCodes.Ret));

                body.Replace(@return, instructions);
            }
        }

        private void WeaveTraceEnterWithParameters(MethodBody body)
        {
            /* TRACE ENTRY: 
             * What we'd like to achieve is this:
             * var paramNames = new string[] { "param1", "param2" }
             * var paramValues = new object[] { param1, param2 }
             * _log.TraceCallEnter("MethodFullName", paramNames, paramValues)
             * var startTick = Stopwatch.GetTimestamp();
             * ...(existing code)...
             */
            var instructions = new List<Instruction>();

            //Declare local variables for the two arrays
            var paramNamesDef = new VariableDefinition("paramNames", TypeReferenceProvider.StringArray);
            var paramValuesDef = new VariableDefinition("paramValues", TypeReferenceProvider.ObjectArray);
            body.Variables.Add(paramNamesDef);
            body.Variables.Add(paramValuesDef);

            int arraySize = body.Method.Parameters.Count(param => !param.IsOut);
            instructions.AddRange(new[]
            {
                Instruction.Create(OpCodes.Ldc_I4, arraySize),  //setArraySize
                Instruction.Create(OpCodes.Newarr, TypeReferenceProvider.String), //create name array
                Instruction.Create(OpCodes.Stloc, paramNamesDef), //store it in local variable
                Instruction.Create(OpCodes.Ldc_I4, arraySize),  //setArraySize
                Instruction.Create(OpCodes.Newarr, TypeReferenceProvider.Object), //create value array
                Instruction.Create(OpCodes.Stloc, paramValuesDef), //store it in local variable
            });

            instructions.AddRange(BuildInstructionsToCopyParameterNamesAndValues(body.Method.Parameters, paramNamesDef, paramValuesDef));

            var logTraceEnterMethod = MethodReferenceProvider.GetTraceEnterWithParametersReference();
            instructions.AddRange(new[]
            {
                Instruction.Create(OpCodes.Ldsfld, StaticLogger),
                Instruction.Create(OpCodes.Ldstr, body.Method.FullName),
                Instruction.Create(OpCodes.Ldloc, paramNamesDef),
                Instruction.Create(OpCodes.Ldloc, paramValuesDef),
                Instruction.Create(OpCodes.Callvirt, logTraceEnterMethod)
            });

            var getTimestampMethod = MethodReferenceProvider.GetTimestampReference();
            var startTickVariable = new VariableDefinition("startTick", TypeReferenceProvider.Long);
            body.Variables.Add(startTickVariable);
            instructions.AddRange(new[]
            {
                Instruction.Create(OpCodes.Call, getTimestampMethod),
                Instruction.Create(OpCodes.Stloc, startTickVariable)
            });

            body.InsertAtTheBeginning(instructions);
        }

        private void WeaveTraceEnterWithoutParameters(MethodBody body)
        {
            /* TRACE ENTRY: 
             * What we'd like to achieve is this:
             * _log.TraceCallEnter("MethodFullName")
             * var startTick = Stopwatch.GetTimestamp();
             * ...(existing code)...
             */
            var instructions = new List<Instruction>();

            var logTraceEnterMethod = MethodReferenceProvider.GetTraceEnterWithoutParametersReference();
            instructions.AddRange(new[]
            {
                Instruction.Create(OpCodes.Ldsfld, StaticLogger),
                Instruction.Create(OpCodes.Ldstr, body.Method.FullName),
                Instruction.Create(OpCodes.Callvirt, logTraceEnterMethod)
            });

            var getTimestampMethod = MethodReferenceProvider.GetTimestampReference();
            var startTickVariable = new VariableDefinition("startTick", TypeReferenceProvider.Long);
            body.Variables.Add(startTickVariable);
            instructions.AddRange(new[]
            {
                Instruction.Create(OpCodes.Call, getTimestampMethod),
                Instruction.Create(OpCodes.Stloc, startTickVariable)
            });
            
            body.InsertAtTheBeginning(instructions);
        }


        private IEnumerable<Instruction> BuildInstructionsToCopyParameterNamesAndValues(Collection<ParameterDefinition> parameters,
            VariableDefinition paramNamesDef, VariableDefinition paramValuesDef)
        {
            var instructions = new List<Instruction>();
            var targetIdx = 0;
            //fill name array and param array
            for (int inputIdx = 0; inputIdx < parameters.Count; inputIdx++)
            {
                var parameter = parameters[inputIdx];

                if (parameter.IsOut) continue; //skip out parameters
                
                //set name at index
                instructions.AddRange(new[]
                {
                    Instruction.Create(OpCodes.Ldloc, paramNamesDef),
                    Instruction.Create(OpCodes.Ldc_I4, targetIdx),
                    Instruction.Create(OpCodes.Ldstr, parameter.Name),
                    Instruction.Create(OpCodes.Stelem_Ref)
                });

                //set value at index
                instructions.Add(Instruction.Create(OpCodes.Ldloc, paramValuesDef));
                instructions.Add(Instruction.Create(OpCodes.Ldc_I4, targetIdx));
                instructions.Add(Instruction.Create(OpCodes.Ldarg, parameter));

                //box if necessary
                if (parameter.ParameterType.IsPrimitive || parameter.ParameterType.IsGenericParameter)
                {
                    instructions.Add(Instruction.Create(OpCodes.Box, parameter.ParameterType));
                }

                instructions.Add(Instruction.Create(OpCodes.Stelem_Ref));

                targetIdx++;
            }

            return instructions;
        }

        private void SearchForAndChangeStaticLogCalls(MethodBody body)
        {
            //look for static log calls
            foreach (var instruction in body.Instructions.ToList()) //create a copy of the instructions so we can update the original
            {
                //TODO test if the instruction is really a static call
                var methodReference = instruction.Operand as MethodReference;
                if (methodReference != null && IsStaticLogTypeOrItsInnerType(methodReference.DeclaringType))
                {
                    //change the call
                    if (!methodReference.HasParameters)
                    {
                        ChangeStaticLogCallWithoutParameter(body, instruction);
                    }
                    else
                    {
                        ChangeStaticLogCallWithParameter(body, instruction);
                    }
                }
            }
        }

        ///The way how we solve this is a bit lame, but fairly simple. We store all parameters into local variables
        /// then call the instance log method reading the parameters from these variables.
        /// A better solution would be to figure out where the call really begins (where is the bottom of the stack)
        /// and insert the instance ref there plus change the call instraction
        private void ChangeStaticLogCallWithParameter(MethodBody body, Instruction oldInstruction)
        {
            var methodReference = (MethodReference)oldInstruction.Operand;
            var instructions = new List<Instruction>();
            var parameters = methodReference.Parameters;

            //create variables to store parameters and push values into them
            var variables = new VariableDefinition[parameters.Count];

            for (int idx = 0; idx < parameters.Count; idx++)
            {
                variables[idx] = new VariableDefinition(parameters[idx].ParameterType);    
                body.Variables.Add(variables[idx]);
                instructions.Add(Instruction.Create(OpCodes.Stloc, variables[idx]));
            }

            //build-up instance call
            instructions.Add(Instruction.Create(OpCodes.Ldsfld, StaticLogger));
            instructions.Add(Instruction.Create(OpCodes.Ldstr, body.Method.FullName));

            for (int idx = parameters.Count-1; idx >= 0; idx--)
            {
                instructions.Add(Instruction.Create(OpCodes.Ldloc, variables[idx]));
            }

            var instanceLogMethod = MethodReferenceProvider.GetInstanceLogMethodWithParameter(GetInstanceLogMethodName(methodReference),
                parameters);

            instructions.Add(Instruction.Create(OpCodes.Callvirt, instanceLogMethod));

            body.Replace(oldInstruction, instructions);
        }

        private void ChangeStaticLogCallWithoutParameter(MethodBody body, Instruction oldInstruction)
        {
            var methodReference = (MethodReference)oldInstruction.Operand;

            var instructions = new List<Instruction>(); 

            var instanceLogMethod = MethodReferenceProvider.GetInstanceLogMethodWithoutParameter(GetInstanceLogMethodName(methodReference));

            instructions.AddRange(new[]
            {
                Instruction.Create(OpCodes.Ldsfld, StaticLogger),
                Instruction.Create(OpCodes.Ldstr, body.Method.FullName),
                Instruction.Create(OpCodes.Callvirt, instanceLogMethod)
            });

            body.Replace(oldInstruction, instructions);
        }
        
        private bool IsStaticLogTypeOrItsInnerType(TypeReference typeReference)
        {
            //TODO check for inner types
            return typeReference.FullName == TypeReferenceProvider.StaticLogReference.FullName;
        }

        private string GetInstanceLogMethodName(MethodReference methodReference)
        {
            //TODO chain inner types in name
            var typeName = methodReference.DeclaringType.Name;
            return typeName + methodReference.Name;
        }

        private TypeReferenceProvider TypeReferenceProvider
        {
            get { return _typeReferenceProvider; }
        }

        private MethodReferenceProvider MethodReferenceProvider
        {
            get { return _methodReferenceProvider; }
        }

        private FieldDefinition StaticLogger
        {
            get { return _staticLoggerField.Value; }
        }

        private FieldDefinition CreateLoggerStaticField()
        {
            //TODO check for existing logger
            var logTypeRef = TypeReferenceProvider.LogAdapterReference;
            var logManagerTypeRef = TypeReferenceProvider.LogManagerReference;

            //look for existing one
            var loggerField = _typeDefinition.Fields.FirstOrDefault(fld => fld.IsStatic && fld.FieldType == logTypeRef);

            if (loggerField == null)
            {
                //TODO check if the _log name is used for something else and use a unqiue name
                loggerField = new FieldDefinition("_log", FieldAttributes.Public | FieldAttributes.Static, logTypeRef); //todo private field?
                _typeDefinition.Fields.Add(loggerField);

                //create field init
                var staticConstructor = _typeDefinition.GetStaticConstructor();
                if (staticConstructor == null)
                {
                    const MethodAttributes methodAttributes = MethodAttributes.Private | MethodAttributes.Static | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;
                    staticConstructor = new MethodDefinition(".cctor", methodAttributes, TypeReferenceProvider.Void);
                    _typeDefinition.Methods.Add(staticConstructor);
                }

                var getLoggerMethod = new MethodReference("GetLogger", logTypeRef, logManagerTypeRef);
                getLoggerMethod.Parameters.Add(new ParameterDefinition(TypeReferenceProvider.Type));

                //build up typeInfo
                var getTypeFromHandleMethod = MethodReferenceProvider.GetGetTypeFromHandleReference();

                staticConstructor.Body.InsertAtTheBeginning(new[]
                    {
                        Instruction.Create(OpCodes.Ldtoken, _typeDefinition),
                        Instruction.Create(OpCodes.Call, getTypeFromHandleMethod),
                        Instruction.Create(OpCodes.Call, getLoggerMethod),
                        Instruction.Create(OpCodes.Stsfld, loggerField),
                        Instruction.Create(OpCodes.Ret)
                    });
            }

            return loggerField;
        }
    }
}
