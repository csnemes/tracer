using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
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
    internal class TypeWeaver : MethodWeaver.ILoggerProvider
    {
        private readonly TypeDefinition _typeDefinition;
        private readonly ITraceLoggingFilter _filter;
        private readonly TypeReferenceProvider _typeReferenceProvider;
        private readonly MethodReferenceProvider _methodReferenceProvider;
        private readonly Lazy<FieldReference> _staticLoggerField;
        private readonly MethodWeaverFactory _methodWeaverFactory;

        internal TypeWeaver(ITraceLoggingFilter filter, TypeReferenceProvider typeReferenceProvider, MethodReferenceProvider methodReferenceProvider,
            TypeDefinition typeDefinition)
        {
            _filter = filter;
            _typeReferenceProvider = typeReferenceProvider;
            _methodReferenceProvider = methodReferenceProvider;
            _typeDefinition = typeDefinition;
            _staticLoggerField = new Lazy<FieldReference>(CreateLoggerStaticField);
            _methodWeaverFactory = new MethodWeaverFactory(typeReferenceProvider, methodReferenceProvider, this);
        }

        public void Execute()
        {
            foreach (var method in _typeDefinition.GetMethods().Where(method => method.HasBody && !method.IsAbstract).ToList())
            {
                if (AlreadyWeaved(method)) continue;
                
                bool shouldAddTrace = _filter.ShouldAddTrace(method);
               
                _methodWeaverFactory.Create(method).Execute(shouldAddTrace);
            }
        }

        private bool AlreadyWeaved(MethodDefinition method)
        {
            //TODO
            return false;
        }

        private void WeaveTraceLeave(MethodBody body)
        {
            //----------------------

            /* TRACE Leave: 
               * What we'd like to achieve is:
               * ...(existing code)... 
               *  long methodTimeInTicks = Stopwatch.GetTimestamp() - startTick;
               * _log.TraceLeave("MethodName", methodTimeInTicks, returnValue)
               * 
            */
            VariableDefinition paramNamesDef = null;
            VariableDefinition paramValuesDef = null;

            var returnType = body.Method.ReturnType;
            bool hasReturnValue = (returnType.MetadataType != MetadataType.Void);
            int numberOfOutParams = body.Method.Parameters.Count(param => param.IsOut);

            VariableDefinition returnValueDef = null;
            var startTickVar = body.Variables.First(var => var.Name.Equals("$startTick"));

            if (hasReturnValue)
            {
                //Declare local variable for the return value
                returnValueDef = body.GetOrDeclareVariable("$returnValue", returnType);
            }
            
            var allReturns = body.Instructions.Where(instr => instr.OpCode == OpCodes.Ret).ToList();

            foreach (var @return in allReturns)
            {
                var instructions = new List<Instruction>();

                if (hasReturnValue)
                {
                    instructions.Add(Instruction.Create(OpCodes.Stloc, returnValueDef)); //store it in local variable
                }

                if (hasReturnValue || numberOfOutParams > 0)
                {
                    //Get local variables for the arrays or declare them if they not exist
                    paramNamesDef = body.GetOrDeclareVariable("$paramNames", TypeReferenceProvider.StringArray);
                    paramValuesDef = body.GetOrDeclareVariable("$paramValues", TypeReferenceProvider.ObjectArray);

                    instructions.AddRange(new[]
                    {
                        Instruction.Create(OpCodes.Ldc_I4, numberOfOutParams + (hasReturnValue ? 1 : 0)),  //setArraySize
                        Instruction.Create(OpCodes.Newarr, TypeReferenceProvider.String), //create name array
                        Instruction.Create(OpCodes.Stloc, paramNamesDef), //store it in local variable
                        Instruction.Create(OpCodes.Ldc_I4, numberOfOutParams + (hasReturnValue ? 1 : 0)),  //setArraySize
                        Instruction.Create(OpCodes.Newarr, TypeReferenceProvider.Object), //create value array
                        Instruction.Create(OpCodes.Stloc, paramValuesDef), //store it in local variable
                    });

                    if (hasReturnValue)
                    {
                        //set name at index
                        instructions.AddRange(new[]
                        {
                            Instruction.Create(OpCodes.Ldloc, paramNamesDef),
                            Instruction.Create(OpCodes.Ldc_I4, 0),
                            Instruction.Create(OpCodes.Ldnull),
                            Instruction.Create(OpCodes.Stelem_Ref)
                        });

                        //set value at index
                        instructions.Add(Instruction.Create(OpCodes.Ldloc, paramValuesDef));
                        instructions.Add(Instruction.Create(OpCodes.Ldc_I4, 0));
                        instructions.Add(Instruction.Create(OpCodes.Ldloc, returnValueDef));

                        //box if necessary
                        if (returnType.IsPrimitive || returnType.IsGenericParameter)
                        {
                            instructions.Add(Instruction.Create(OpCodes.Box, returnType));
                        }

                        instructions.Add(Instruction.Create(OpCodes.Stelem_Ref));
                    }

                    instructions.AddRange(
                        BuildInstructionsToCopyParameterNamesAndValues(body.Method.Parameters.Where(p => p.IsOut),
                        paramNamesDef, paramValuesDef, hasReturnValue ? 1 : 0));
                }

                //build up Trace call
                instructions.Add(Instruction.Create(OpCodes.Ldsfld, StaticLogger));

                instructions.AddRange(LoadMethodNameOnStack(body.Method));

                //calculate ticks elapsed
                var getTimestampMethod = MethodReferenceProvider.GetTimestampReference();
                instructions.Add(Instruction.Create(OpCodes.Call, getTimestampMethod));
                instructions.Add(Instruction.Create(OpCodes.Ldloc, startTickVar));
                instructions.Add(Instruction.Create(OpCodes.Sub));


                if (hasReturnValue || numberOfOutParams > 0)
                {
                    instructions.AddRange(new[]
                    {
                        Instruction.Create(OpCodes.Ldloc, paramNamesDef),
                        Instruction.Create(OpCodes.Ldloc, paramValuesDef),
                    });
                }
                else
                {
                    instructions.AddRange(new[]
                    {
                        Instruction.Create(OpCodes.Ldnull),
                        Instruction.Create(OpCodes.Ldnull),
                    });
                }

                instructions.Add(Instruction.Create(OpCodes.Callvirt, MethodReferenceProvider.GetTraceLeaveWithReturnValueReference()));

                //return with original value
                if (hasReturnValue)
                {
                    instructions.Add(Instruction.Create(OpCodes.Ldloc, returnValueDef)); //read from local variable
                }

                instructions.Add(Instruction.Create(OpCodes.Ret));

                body.Replace(@return, instructions);
            }
        }



        private void WeaveTraceEnter(MethodBody body)
        {
            /* TRACE ENTRY: 
             * What we'd like to achieve is this:
             * var paramNames = new string[] { "param1", "param2" }
             * var paramTypes = new string[] { "int", "string" }
             * var paramValues = new object[] { param1, param2 }
             * _log.TraceCallEnter("MethodName", paramNames, paramTypes, paramValues)
             * var startTick = Stopwatch.GetTimestamp();
             * ...(existing code)...
             */
            int numberOfLoggableParams = body.Method.Parameters.Count(param => !param.IsOut);
            bool hasLoggableParams = numberOfLoggableParams > 0;

            var instructions = new List<Instruction>();
            VariableDefinition paramNamesDef = null;
            VariableDefinition paramValuesDef = null;
            
            if (hasLoggableParams)
            {
                //Declare local variables for the arrays
                paramNamesDef = body.GetOrDeclareVariable("$paramNames", TypeReferenceProvider.StringArray);
                paramValuesDef = body.GetOrDeclareVariable("$paramValues", TypeReferenceProvider.ObjectArray);

                instructions.AddRange(new[]
                {
                    Instruction.Create(OpCodes.Ldc_I4, numberOfLoggableParams),  //setArraySize
                    Instruction.Create(OpCodes.Newarr, TypeReferenceProvider.String), //create name array
                    Instruction.Create(OpCodes.Stloc, paramNamesDef), //store it in local variable
                    Instruction.Create(OpCodes.Ldc_I4, numberOfLoggableParams),  //setArraySize
                    Instruction.Create(OpCodes.Newarr, TypeReferenceProvider.Object), //create value array
                    Instruction.Create(OpCodes.Stloc, paramValuesDef), //store it in local variable
                });

                instructions.AddRange(BuildInstructionsToCopyParameterNamesAndValues(
                    body.Method.Parameters.Where(p => !p.IsOut), paramNamesDef, paramValuesDef, 0));
            }

            //build up logger call
            instructions.Add(Instruction.Create(OpCodes.Ldsfld, StaticLogger));
            instructions.AddRange(LoadMethodNameOnStack(body.Method));

            if (hasLoggableParams)
            {
                instructions.AddRange(new[]
                {
                    Instruction.Create(OpCodes.Ldloc, paramNamesDef),
                    Instruction.Create(OpCodes.Ldloc, paramValuesDef),
                });
            }
            else
            {
                instructions.AddRange(new[]
                {
                    Instruction.Create(OpCodes.Ldnull),
                    Instruction.Create(OpCodes.Ldnull),
                });
            }

            instructions.Add(Instruction.Create(OpCodes.Callvirt, MethodReferenceProvider.GetTraceEnterWithParametersReference()));


            //timer start
            var startTickVariable = body.GetOrDeclareVariable("$startTick", TypeReferenceProvider.Long);
            instructions.AddRange(new[]
            {
                Instruction.Create(OpCodes.Call, MethodReferenceProvider.GetTimestampReference()),
                Instruction.Create(OpCodes.Stloc, startTickVariable)
            });

            body.InsertAtTheBeginning(instructions);
        }

        private IEnumerable<Instruction> LoadMethodNameOnStack(MethodDefinition method)
        {
            var sb = new StringBuilder();
            sb.Append(method.Name);
            sb.Append("(");
            for (int i = 0; i < method.Parameters.Count; i++)
            {
                var paramDef = method.Parameters[i];
                if (paramDef.IsOut) sb.Append("out ");
                sb.Append(paramDef.ParameterType.Name);
                if (i < method.Parameters.Count - 1) sb.Append(", ");
            }
            sb.Append(")");

            return new[]
            {
                Instruction.Create(OpCodes.Ldstr, sb.ToString())
            };
        }

        private IEnumerable<Instruction> BuildInstructionsToCopyParameterNamesAndValues(IEnumerable<ParameterDefinition> parameters,
                VariableDefinition paramNamesDef, VariableDefinition paramValuesDef, int startingIndex)
        {
            var instructions = new List<Instruction>();

            foreach (var parameter in parameters)
            {
                //set name at index
                instructions.AddRange(new[]
                {
                    Instruction.Create(OpCodes.Ldloc, paramNamesDef),
                    Instruction.Create(OpCodes.Ldc_I4, startingIndex),
                    Instruction.Create(OpCodes.Ldstr, parameter.Name),
                    Instruction.Create(OpCodes.Stelem_Ref)
                });

                //set value at index
                instructions.Add(Instruction.Create(OpCodes.Ldloc, paramValuesDef));
                instructions.Add(Instruction.Create(OpCodes.Ldc_I4, startingIndex));
                instructions.Add(Instruction.Create(OpCodes.Ldarg, parameter));

                if (parameter.IsOut)
                {
                    switch (parameter.ParameterType.MetadataType)
                    {
                        case MetadataType.Int16:
                            instructions.Add(Instruction.Create(OpCodes.Ldind_I2));
                            break;
                        case MetadataType.Int32:
                            instructions.Add(Instruction.Create(OpCodes.Ldind_I4));
                            break;
                        case MetadataType.Int64:
                            instructions.Add(Instruction.Create(OpCodes.Ldind_I8));
                            break;
                        case MetadataType.UInt16:
                            instructions.Add(Instruction.Create(OpCodes.Ldind_U2));
                            break;
                        case MetadataType.UInt32:
                            instructions.Add(Instruction.Create(OpCodes.Ldind_U4));
                            break;
                        case MetadataType.UInt64:
                            instructions.Add(Instruction.Create(OpCodes.Ldind_I8));
                            break;
                        default:
                            instructions.Add(Instruction.Create(OpCodes.Ldind_Ref));
                            break;
                    }
                }

                //box if necessary
                if (parameter.ParameterType.IsPrimitive || parameter.ParameterType.IsGenericParameter)
                {
                    instructions.Add(Instruction.Create(OpCodes.Box, parameter.ParameterType));
                }

                instructions.Add(Instruction.Create(OpCodes.Stelem_Ref));

                startingIndex++;
            }

            return instructions;
        }

        private void SearchForAndReplaceStaticLogCalls(MethodBody body)
        {
            //look for static log calls
            foreach (var instruction in body.Instructions.ToList()) //create a copy of the instructions so we can update the original
            {
                var methodReference = instruction.Operand as MethodReference;
                if (instruction.OpCode == OpCodes.Call && methodReference != null &&  IsStaticLogTypeOrItsInnerType(methodReference.DeclaringType))
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
            var instructions = new List<Instruction>();
            var methodReference = (MethodReference)oldInstruction.Operand;

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
            instructions.AddRange(LoadMethodNameOnStack(body.Method));

            for (int idx = parameters.Count-1; idx >= 0; idx--)
            {
                instructions.Add(Instruction.Create(OpCodes.Ldloc, variables[idx]));
            }

            instructions.Add(Instruction.Create(OpCodes.Callvirt, MethodReferenceProvider.GetInstanceLogMethodWithParameter(GetInstanceLogMethodName(methodReference), parameters)));

            body.Replace(oldInstruction, instructions);
        }

        private void ChangeStaticLogCallWithoutParameter(MethodBody body, Instruction oldInstruction)
        {
            var instructions = new List<Instruction>();
            
            var methodReference = (MethodReference)oldInstruction.Operand;

            instructions.Add(Instruction.Create(OpCodes.Ldsfld, StaticLogger));
            instructions.AddRange(LoadMethodNameOnStack(body.Method));
            instructions.Add(Instruction.Create(OpCodes.Callvirt, MethodReferenceProvider.GetInstanceLogMethodWithoutParameter(GetInstanceLogMethodName(methodReference))));

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

        public FieldReference StaticLogger
        {
            get { return _staticLoggerField.Value; }
        }

        private FieldReference CreateLoggerStaticField()
        {
            //TODO check for existing logger
            var logTypeRef = TypeReferenceProvider.LogAdapterReference;
            var logManagerTypeRef = TypeReferenceProvider.LogManagerReference;

            //look for existing one
            var loggerField = _typeDefinition.Fields.FirstOrDefault(fld => fld.IsStatic && fld.FieldType == logTypeRef);

            if (loggerField != null) return loggerField.FixFieldReferenceIfDeclaringTypeIsGeneric();

            //$log should be unique
            loggerField = new FieldDefinition("$log", FieldAttributes.Private | FieldAttributes.Static, logTypeRef); 
            _typeDefinition.Fields.Add(loggerField);

            //create field init
            var staticConstructor = _typeDefinition.GetStaticConstructor();
            if (staticConstructor == null)
            {
                const MethodAttributes methodAttributes = MethodAttributes.Private | MethodAttributes.Static | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;
                staticConstructor = new MethodDefinition(".cctor", methodAttributes, TypeReferenceProvider.Void);
                _typeDefinition.Methods.Add(staticConstructor);
                staticConstructor.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
            }

            var getLoggerMethod = new MethodReference("GetLogger", logTypeRef, logManagerTypeRef);
            getLoggerMethod.Parameters.Add(new ParameterDefinition(TypeReferenceProvider.Type));

            //build up typeInfo
            var getTypeFromHandleMethod = MethodReferenceProvider.GetGetTypeFromHandleReference();

            //spec treatment for generic types 
            var loggerFieldRef = loggerField.FixFieldReferenceIfDeclaringTypeIsGeneric();

            staticConstructor.Body.InsertAtTheBeginning(new[]
            {
                Instruction.Create(OpCodes.Ldtoken, _typeDefinition.GetGenericInstantiationIfGeneric()),
                Instruction.Create(OpCodes.Call, getTypeFromHandleMethod),
                Instruction.Create(OpCodes.Call, getLoggerMethod),
                Instruction.Create(OpCodes.Stsfld, loggerFieldRef)
            });

            return loggerFieldRef;
        }
    }
}
