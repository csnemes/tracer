using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using Tracer.Fody.Helpers;

namespace Tracer.Fody.Weavers
{
    /// <summary>
    /// Weaver for a single method
    /// </summary>
    internal class MethodWeaver
    {
        private readonly TypeReferenceProvider _typeReferenceProvider;
        private readonly MethodReferenceProvider _methodReferenceProvider;
        private readonly MethodDefinition _methodDefinition;
        private readonly MethodBody _body;
        private readonly ILoggerProvider _loggerProvider;

        internal MethodWeaver(TypeReferenceProvider typeReferenceProvider, MethodReferenceProvider methodReferenceProvider,
            ILoggerProvider loggerProvider, MethodDefinition methodDefinition)
        {
            _typeReferenceProvider = typeReferenceProvider;
            _methodReferenceProvider = methodReferenceProvider;
            _methodDefinition = methodDefinition;
            _body = methodDefinition.Body;
            _loggerProvider = loggerProvider;
        }

        /// <summary>
        /// Runs the method weaver which adds trace logs if required and rewrites static log calls
        /// </summary>
        /// <param name="addTrace">if true trace logs are added</param>
        public void Execute(bool addTrace)
        {
            _body.SimplifyMacros();

            if (addTrace)
            {
                WeaveTraceEnter();
                WeaveTraceLeave();
            }

            SearchForAndReplaceStaticLogCalls();

            _body.OptimizeMacros();
        }

        private void WeaveTraceEnter()
        {
            /* TRACE ENTRY: 
             * What we'd like to achieve is this:
             * var paramNames = new string[] { "param1", "param2" }
             * var paramValues = new object[] { param1, param2 }
             * _log.TraceCallEnter("MethodName", paramNames, paramTypes, paramValues)
             * var startTick = Stopwatch.GetTimestamp();
             * ...(existing code)...
             */
            var instructions = new List<Instruction>();
            VariableDefinition paramNamesDef = null;
            VariableDefinition paramValuesDef = null;

            var  traceEnterNeedsParamArray = _body.Method.Parameters.Any(param => !param.IsOut); 
            var  traceEnterParamArraySize = _body.Method.Parameters.Count(param => !param.IsOut); 

            if (traceEnterNeedsParamArray)
            {
                //Declare local variables for the arrays
                paramNamesDef = _body.GetOrDeclareVariable("$paramNames", _typeReferenceProvider.StringArray);
                paramValuesDef = _body.GetOrDeclareVariable("$paramValues", _typeReferenceProvider.ObjectArray);

                instructions.AddRange(InitArray(paramNamesDef, traceEnterParamArraySize, _typeReferenceProvider.String));
                instructions.AddRange(InitArray(paramValuesDef, traceEnterParamArraySize, _typeReferenceProvider.Object));

                instructions.AddRange(BuildInstructionsToCopyParameterNamesAndValues(
                    _body.Method.Parameters.Where(p => !p.IsOut), paramNamesDef, paramValuesDef, 0));
            }

            //build up logger call
            instructions.Add(Instruction.Create(OpCodes.Ldsfld, _loggerProvider.StaticLogger));
            instructions.AddRange(LoadMethodNameOnStack());
            instructions.Add(traceEnterNeedsParamArray ? Instruction.Create(OpCodes.Ldloc, paramNamesDef) : Instruction.Create(OpCodes.Ldnull));
            instructions.Add(traceEnterNeedsParamArray ? Instruction.Create(OpCodes.Ldloc, paramValuesDef) : Instruction.Create(OpCodes.Ldnull));
            instructions.Add(Instruction.Create(OpCodes.Callvirt, _methodReferenceProvider.GetTraceEnterWithParametersReference()));

            //timer start
            var startTickVariable = _body.GetOrDeclareVariable("$startTick", _typeReferenceProvider.Long);
            instructions.AddRange(new[]
            {
                Instruction.Create(OpCodes.Call, _methodReferenceProvider.GetTimestampReference()),
                Instruction.Create(OpCodes.Stloc, startTickVariable)
            });

            _body.InsertAtTheBeginning(instructions);
        }

        private void WeaveTraceLeave()
        {
            //----------------------

            /* TRACE Leave: 
               * What we'd like to achieve is:
               * ...(existing code)... 
               *  long methodTimeInTicks = Stopwatch.GetTimestamp() - startTick;
               * _log.TraceLeave("MethodName", methodTimeInTicks, returnValue)
               * 
            */
            VariableDefinition returnValueDef = null;

            if (HasReturnValue)
            {
                //Declare local variable for the return value
                returnValueDef = _body.GetOrDeclareVariable("$returnValue", ReturnType);
            }

            var allReturns = _body.Instructions.Where(instr => instr.OpCode == OpCodes.Ret).ToList();

            foreach (var @return in allReturns)
            {
                ProcessSingleReturn(@return, returnValueDef);
            }
        }

        private void ProcessSingleReturn(Instruction @return, VariableDefinition returnValueDef)
        {
            var instructions = new List<Instruction>();
            VariableDefinition paramNamesDef = null;
            VariableDefinition paramValuesDef = null;
            
            var traceLeaveNeedsParamArray = (HasReturnValue || _body.Method.Parameters.Any(param => param.IsOut || param.ParameterType.IsByReference));
            var traceLeaveParamArraySize = _body.Method.Parameters.Count(param => param.IsOut || param.ParameterType.IsByReference) + (HasReturnValue ? 1 : 0); 

            if (HasReturnValue)
            {
                instructions.Add(Instruction.Create(OpCodes.Stloc, returnValueDef)); //store it in local variable
            }

            if (traceLeaveNeedsParamArray)
            {
                //Get local variables for the arrays or declare them if they not exist
                paramNamesDef = _body.GetOrDeclareVariable("$paramNames", _typeReferenceProvider.StringArray);
                paramValuesDef = _body.GetOrDeclareVariable("$paramValues", _typeReferenceProvider.ObjectArray);

                //init arrays
                instructions.AddRange(InitArray(paramNamesDef, traceLeaveParamArraySize, _typeReferenceProvider.String));
                instructions.AddRange(InitArray(paramValuesDef, traceLeaveParamArraySize, _typeReferenceProvider.Object));

                if (HasReturnValue)
                {
                    instructions.AddRange(StoreValueReadByInstructionsInArray(paramNamesDef, 0, Instruction.Create(OpCodes.Ldnull)));
                    instructions.AddRange(StoreVariableInObjectArray(paramValuesDef, 0, returnValueDef));
                }

                instructions.AddRange(
                    BuildInstructionsToCopyParameterNamesAndValues(_body.Method.Parameters.Where(p => p.IsOut || p.ParameterType.IsByReference),
                        paramNamesDef, paramValuesDef, HasReturnValue ? 1 : 0));
            }

            //build up Trace call
            instructions.Add(Instruction.Create(OpCodes.Ldsfld, _loggerProvider.StaticLogger));
            instructions.AddRange(LoadMethodNameOnStack());
            //calculate ticks elapsed
            instructions.Add(Instruction.Create(OpCodes.Call, _methodReferenceProvider.GetTimestampReference()));
            instructions.Add(Instruction.Create(OpCodes.Ldloc, _body.GetVariable("$startTick")));
            instructions.Add(Instruction.Create(OpCodes.Sub));
            //tick calc ends
            instructions.Add(traceLeaveNeedsParamArray ? Instruction.Create(OpCodes.Ldloc, paramNamesDef) : Instruction.Create(OpCodes.Ldnull));
            instructions.Add(traceLeaveNeedsParamArray ? Instruction.Create(OpCodes.Ldloc, paramValuesDef) : Instruction.Create(OpCodes.Ldnull));
            instructions.Add(Instruction.Create(OpCodes.Callvirt, _methodReferenceProvider.GetTraceLeaveWithReturnValueReference()));

            //return with original value
            if (HasReturnValue)
            {
                instructions.Add(Instruction.Create(OpCodes.Ldloc, returnValueDef)); //read from local variable
            }
            instructions.Add(Instruction.Create(OpCodes.Ret));

            _body.Replace(@return, instructions);
        }

        private void SearchForAndReplaceStaticLogCalls()
        {
            //look for static log calls
            foreach (var instruction in _body.Instructions.ToList()) //create a copy of the instructions so we can update the original
            {
                var methodReference = instruction.Operand as MethodReference;
                if (instruction.OpCode == OpCodes.Call && methodReference != null && IsStaticLogTypeOrItsInnerType(methodReference.DeclaringType))
                {
                    //change the call
                    if (!methodReference.HasParameters)
                    {
                        ChangeStaticLogCallWithoutParameter(instruction);
                    }
                    else
                    {
                        ChangeStaticLogCallWithParameter(instruction);
                    }
                }
            }
        }

        private TypeReference ReturnType
        {
            get { return _methodDefinition.ReturnType; }
        }

        private bool HasReturnValue
        {
            get { return (ReturnType.MetadataType != MetadataType.Void); }
        }

        private IEnumerable<Instruction> InitArray(VariableDefinition arrayVar, int size, TypeReference type)
        {
            yield return Instruction.Create(OpCodes.Ldc_I4, size);  //setArraySize
            yield return Instruction.Create(OpCodes.Newarr, type); //create name array
            yield return Instruction.Create(OpCodes.Stloc, arrayVar); //store it in local variable
        }

        private IEnumerable<Instruction> StoreValueReadByInstructionsInArray(VariableDefinition arrayVar, int position, params Instruction[] putValueOnStack)
        {
            yield return Instruction.Create(OpCodes.Ldloc, arrayVar);
            yield return Instruction.Create(OpCodes.Ldc_I4, position);
            foreach (var instruction in putValueOnStack)
            {
                yield return instruction;
            }
            yield return Instruction.Create(OpCodes.Stelem_Ref);
        }

        private IEnumerable<Instruction> StoreVariableInObjectArray(VariableDefinition arrayVar, int position, VariableDefinition variable)
        {
            var varType = variable.VariableType;
            yield return Instruction.Create(OpCodes.Ldloc, arrayVar);
            yield return Instruction.Create(OpCodes.Ldc_I4, position);
            yield return Instruction.Create(OpCodes.Ldloc, variable);
            //box if necessary
            if (IsBoxingNeeded(varType))
            {
                yield return Instruction.Create(OpCodes.Box, varType);
            }
            yield return Instruction.Create(OpCodes.Stelem_Ref);
        }

        private static bool IsBoxingNeeded(TypeReference type)
        {
            return type.IsPrimitive || type.IsGenericParameter || type.IsValueType;
        }

        private IEnumerable<Instruction> StoreParameterInObjectArray(VariableDefinition arrayVar, int position, ParameterDefinition parameter)
        {
            var parameterType = parameter.ParameterType;
            var parameterElementType = parameterType.IsByReference ? ((ByReferenceType) parameterType).ElementType : parameterType;
            yield return Instruction.Create(OpCodes.Ldloc, arrayVar);
            yield return Instruction.Create(OpCodes.Ldc_I4, position);
            yield return Instruction.Create(OpCodes.Ldarg, parameter);

            //check if ref (or out)
            if (parameterType.IsByReference)
            {
                switch (parameterElementType.MetadataType)
                {
                    case MetadataType.ValueType:
                        yield return Instruction.Create(OpCodes.Ldobj, parameterElementType);
                        break;
                    case MetadataType.Int16:
                        yield return Instruction.Create(OpCodes.Ldind_I2);
                        break;
                    case MetadataType.Int32:
                        yield return Instruction.Create(OpCodes.Ldind_I4);
                        break;
                    case MetadataType.Int64:
                    case MetadataType.UInt64:
                        yield return Instruction.Create(OpCodes.Ldind_I8);
                        break;
                    case MetadataType.UInt16:
                        yield return Instruction.Create(OpCodes.Ldind_U2);
                        break;
                    case MetadataType.UInt32:
                        yield return Instruction.Create(OpCodes.Ldind_U4);
                        break;
                    case MetadataType.Single:
                        yield return Instruction.Create(OpCodes.Ldind_R4);
                        break;
                    case MetadataType.Double:
                        yield return Instruction.Create(OpCodes.Ldind_R8);
                        break;
                    case MetadataType.IntPtr:
                        yield return Instruction.Create(OpCodes.Ldind_I);
                        break;
                    case MetadataType.SByte:
                        yield return Instruction.Create(OpCodes.Ldind_I1);
                        break;
                    case MetadataType.Byte:
                        yield return Instruction.Create(OpCodes.Ldind_U1);
                        break;
                    default:
                        yield return Instruction.Create(OpCodes.Ldind_Ref);
                        break;
                }
            }

            //box if necessary
            if (IsBoxingNeeded(parameterElementType))
            {
                yield return Instruction.Create(OpCodes.Box, parameterElementType);
            }
            yield return Instruction.Create(OpCodes.Stelem_Ref);
        }

        private IEnumerable<Instruction> BuildInstructionsToCopyParameterNamesAndValues(IEnumerable<ParameterDefinition> parameters,
        VariableDefinition paramNamesDef, VariableDefinition paramValuesDef, int startingIndex)
        {
            var instructions = new List<Instruction>();
            var index = startingIndex;
            foreach (var parameter in parameters)
            {
                //set name at index
                instructions.AddRange(StoreValueReadByInstructionsInArray(paramNamesDef, index, Instruction.Create(OpCodes.Ldstr, parameter.Name)));
                instructions.AddRange(StoreParameterInObjectArray(paramValuesDef, index, parameter));
                index++;
            }

            return instructions;
        }


        ///The way how we solve this is a bit lame, but fairly simple. We store all parameters into local variables
        /// then call the instance log method reading the parameters from these variables.
        /// A better solution would be to figure out where the call really begins (where is the bottom of the stack)
        /// and insert the instance ref there plus change the call instraction
        private void ChangeStaticLogCallWithParameter(Instruction oldInstruction)
        {
            var instructions = new List<Instruction>();
            var methodReference = (MethodReference)oldInstruction.Operand;

            var parameters = methodReference.Parameters;

            //create variables to store parameters and push values into them
            var variables = new VariableDefinition[parameters.Count];

            for (int idx = 0; idx < parameters.Count; idx++)
            {
                variables[idx] = new VariableDefinition(parameters[idx].ParameterType);
                _body.Variables.Add(variables[idx]);
                instructions.Add(Instruction.Create(OpCodes.Stloc, variables[idx]));
            }

            //build-up instance call
            instructions.Add(Instruction.Create(OpCodes.Ldsfld, _loggerProvider.StaticLogger));
            instructions.AddRange(LoadMethodNameOnStack());

            for (int idx = parameters.Count - 1; idx >= 0; idx--)
            {
                instructions.Add(Instruction.Create(OpCodes.Ldloc, variables[idx]));
            }

            instructions.Add(Instruction.Create(OpCodes.Callvirt, _methodReferenceProvider.GetInstanceLogMethodWithParameter(GetInstanceLogMethodName(methodReference), parameters)));

            _body.Replace(oldInstruction, instructions);
        }

        private void ChangeStaticLogCallWithoutParameter(Instruction oldInstruction)
        {
            var instructions = new List<Instruction>();

            var methodReference = (MethodReference)oldInstruction.Operand;

            instructions.Add(Instruction.Create(OpCodes.Ldsfld, _loggerProvider.StaticLogger));
            instructions.AddRange(LoadMethodNameOnStack());
            instructions.Add(Instruction.Create(OpCodes.Callvirt, _methodReferenceProvider.GetInstanceLogMethodWithoutParameter(GetInstanceLogMethodName(methodReference))));

            _body.Replace(oldInstruction, instructions);
        }

        private bool IsStaticLogTypeOrItsInnerType(TypeReference typeReference)
        {
            //TODO check for inner types
            return typeReference.FullName == _typeReferenceProvider.StaticLogReference.FullName;
        }

        private string GetInstanceLogMethodName(MethodReference methodReference)
        {
            //TODO chain inner types in name
            var typeName = methodReference.DeclaringType.Name;
            return typeName + methodReference.Name;
        }

        private IEnumerable<Instruction> LoadMethodNameOnStack()
        {
            var sb = new StringBuilder();
            sb.Append(_methodDefinition.Name);
            sb.Append("(");
            for (int i = 0; i < _methodDefinition.Parameters.Count; i++)
            {
                var paramDef = _methodDefinition.Parameters[i];
                if (paramDef.IsOut) sb.Append("out ");
                sb.Append(paramDef.ParameterType.Name);
                if (i < _methodDefinition.Parameters.Count - 1) sb.Append(", ");
            }
            sb.Append(")");

            return new[]
            {
                Instruction.Create(OpCodes.Ldstr, sb.ToString())
            };
        }

        internal interface ILoggerProvider
        {
            FieldReference StaticLogger { get; }
        }

    }
}
