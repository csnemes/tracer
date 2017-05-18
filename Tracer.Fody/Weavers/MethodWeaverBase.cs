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
    internal abstract class MethodWeaverBase
    {
        protected readonly ILoggerProvider _loggerProvider;
        protected readonly TypeReferenceProvider _typeReferenceProvider;
        protected readonly MethodReferenceProvider _methodReferenceProvider;
        protected readonly MethodDefinition _methodDefinition;
        protected readonly MethodBody _body;
        protected readonly bool _isEmptyBody;

        protected const string ExceptionMarker = "$exception";
        protected const string StartTickVarName = "$startTick";

        internal MethodWeaverBase(TypeReferenceProvider typeReferenceProvider, MethodReferenceProvider methodReferenceProvider,
                ILoggerProvider loggerProvider, MethodDefinition methodDefinition)
        {
            _typeReferenceProvider = typeReferenceProvider;
            _methodReferenceProvider = methodReferenceProvider;
            _methodDefinition = methodDefinition;
            _body = methodDefinition.Body;
            _isEmptyBody = (_body.Instructions.Count == 0);
            _loggerProvider = loggerProvider;
        }

        private string PrettyMethodName
        {
            get
            {
                //check if method name is generated and prettyfy it
                var position = _methodDefinition.Name.IndexOf(">", StringComparison.OrdinalIgnoreCase);
                return position > 1 ? _methodDefinition.Name.Substring(1, position - 1) : _methodDefinition.Name;
            }
        }

        protected virtual TypeReference ReturnType
        {
            get { return _methodDefinition.ReturnType; }
        }

        protected virtual bool HasReturnValue
        {
            get { return (ReturnType.MetadataType != MetadataType.Void); }
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

            _body.InitLocals = true;
            _body.OptimizeMacros();
        }

        protected abstract void WeaveTraceEnter();

        protected abstract void WeaveTraceLeave();

        protected abstract void SearchForAndReplaceStaticLogCalls();
        
        protected List<Instruction> CreateTraceEnterCallInstructions()
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

            var traceEnterNeedsParamArray = _body.Method.Parameters.Any(param => !param.IsOut);
            var traceEnterParamArraySize = _body.Method.Parameters.Count(param => !param.IsOut);

            if (traceEnterNeedsParamArray)
            {
                //Declare local variables for the arrays
                paramNamesDef = ParamNamesVariable;
                paramValuesDef = ParamValuesVariable;

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
            instructions.Add(Instruction.Create(OpCodes.Callvirt, _methodReferenceProvider.GetTraceEnterReference()));

            //timer start
            instructions.AddRange(new[]
            {
                Instruction.Create(OpCodes.Call, _methodReferenceProvider.GetTimestampReference()),
                Instruction.Create(OpCodes.Stloc, StartTickVariable)
            });

            return instructions;
        }

        private VariableDefinition _paramNamesVariable;

        protected VariableDefinition ParamNamesVariable
        {
            get
            {
                if (_paramNamesVariable == null)
                {
                    _paramNamesVariable = _body.DeclareVariable("$paramNames", _typeReferenceProvider.StringArray);
                }
                return _paramNamesVariable;
            }
        }

        private VariableDefinition _paramValuesVariable;

        protected VariableDefinition ParamValuesVariable
        {
            get
            {
                if (_paramValuesVariable == null)
                {
                    _paramValuesVariable = _body.DeclareVariable("$paramValues", _typeReferenceProvider.ObjectArray);
                }
                return _paramValuesVariable;
            }
        }


        private VariableDefinition _startTickVariable;

        protected VariableDefinition StartTickVariable
        {
            get
            {
                if (_startTickVariable == null)
                {
                    _startTickVariable = _body.DeclareVariable(StartTickVarName, _typeReferenceProvider.Long);
                }
                return _startTickVariable;
            }
        }


        protected IEnumerable<Instruction> InitArray(VariableDefinition arrayVar, int size, TypeReference type)
        {
            yield return Instruction.Create(OpCodes.Ldc_I4, size);  //setArraySize
            yield return Instruction.Create(OpCodes.Newarr, type); //create name array
            yield return Instruction.Create(OpCodes.Stloc, arrayVar); //store it in local variable
        }

        protected IEnumerable<Instruction> StoreValueReadByInstructionsInArray(VariableDefinition arrayVar, int position, params Instruction[] putValueOnStack)
        {
            yield return Instruction.Create(OpCodes.Ldloc, arrayVar);
            yield return Instruction.Create(OpCodes.Ldc_I4, position);
            foreach (var instruction in putValueOnStack)
            {
                yield return instruction;
            }
            yield return Instruction.Create(OpCodes.Stelem_Ref);
        }

        protected IEnumerable<Instruction> StoreVariableInObjectArray(VariableDefinition arrayVar, int position, VariableDefinition variable)
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

        protected static bool IsBoxingNeeded(TypeReference type)
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

        protected IEnumerable<Instruction> BuildInstructionsToCopyParameterNamesAndValues(IEnumerable<ParameterDefinition> parameters,
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

        protected IEnumerable<Instruction> LoadMethodNameOnStack()
        {
            var sb = new StringBuilder();
            sb.Append((string) PrettyMethodName);
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

        protected bool IsStaticLogTypeOrItsInnerType(TypeReference typeReference)
        {
            //TODO check for inner types
            return typeReference.FullName == _typeReferenceProvider.StaticLogReference.FullName;
        }

        protected static VariableDefinition GetVariableDefinitionForType(TypeReference typeRef, MethodReference methodReference, MethodReference hostMethodDefinition)
        {
            var genericParameter = typeRef as GenericParameter;
            //if not generic param or a type defined generic param or generic defined in host method return normally
            if (genericParameter == null || 
                genericParameter.DeclaringType != null ||
                (genericParameter.DeclaringMethod == hostMethodDefinition)) return new VariableDefinition(typeRef);
           
            //generic is defined in the called method
            var genericMethod = methodReference as GenericInstanceMethod;
            if (genericMethod == null) throw new ApplicationException("Generic parameter for a non generic method."); //should not happen

            int idx;
            if (!Int32.TryParse(genericParameter.Name.Replace('!', ' '), out idx))
            {
                throw new ApplicationException(String.Format("Generic parameter {0} cannot be parsed for index.", genericParameter.Name));
            }

            return new VariableDefinition(genericMethod.GenericArguments[idx]);
        }
    }
}
