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
            
            var allReturns = body.Instructions.Where(instr => instr.OpCode == OpCodes.Ret).ToList();

            foreach (var @return in allReturns)
            {
                var instructions = new List<Instruction>();

                instructions.Add(Instruction.Create(OpCodes.Stloc, returnValueDef)); //store it in local variable

                var logTraceLeaveMethod = MethodReferenceProvider.GetTraceLeaveWithReturnValueReference();

                instructions.Add(Instruction.Create(OpCodes.Ldsfld, StaticLogger));
                instructions.Add(Instruction.Create(OpCodes.Ldstr, body.Method.FullName));
                instructions.Add(Instruction.Create(OpCodes.Ldc_I8, 0L));
                instructions.Add(Instruction.Create(OpCodes.Ldloc, returnValueDef));

                //boxing if needed
                if (returnType.IsPrimitive)
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

            foreach (var @return in allReturns)
            {
                var instructions = new List<Instruction>();

                var logTraceLeaveMethod =
                    MethodReferenceProvider.GetTraceLeaveWithoutReturnValueReference();

                instructions.Add(Instruction.Create(OpCodes.Ldsfld, StaticLogger));
                instructions.Add(Instruction.Create(OpCodes.Ldstr, body.Method.FullName));
                instructions.Add(Instruction.Create(OpCodes.Ldc_I8, 0L));
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
                if (parameter.ParameterType.IsPrimitive)
                {
                    instructions.Add(Instruction.Create(OpCodes.Box, parameter.ParameterType));
                }

                instructions.Add(Instruction.Create(OpCodes.Stelem_Ref));

                targetIdx++;
            }

            return instructions;
        }

        private void WeaveLogMethods(MethodBody body)
        {
            
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
                loggerField = new FieldDefinition("_log", FieldAttributes.Public | FieldAttributes.Static, logTypeRef);
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
