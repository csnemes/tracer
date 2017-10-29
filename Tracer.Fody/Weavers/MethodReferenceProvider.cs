using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;
using Mono.Collections.Generic;
using Tracer.Fody.Helpers;

namespace Tracer.Fody.Weavers
{
    internal class MethodReferenceProvider
    {
        private readonly ModuleDefinition _moduleDefinition;
        private readonly TypeReferenceProvider _typeReferenceProvider;

        public MethodReferenceProvider(TypeReferenceProvider typeReferenceProvider, ModuleDefinition moduleDefinition)
        {
            _moduleDefinition = moduleDefinition;
            _typeReferenceProvider = typeReferenceProvider;
        }

        public MethodReference GetTraceEnterReference()
        {
            var logTraceEnterMethod = new MethodReference("TraceEnter", _moduleDefinition.TypeSystem.Void, _typeReferenceProvider.LogAdapterReference);
            logTraceEnterMethod.HasThis = true; //instance method
            logTraceEnterMethod.Parameters.Add(new ParameterDefinition(_moduleDefinition.TypeSystem.String));
            logTraceEnterMethod.Parameters.Add(new ParameterDefinition(_typeReferenceProvider.StringArray));
            logTraceEnterMethod.Parameters.Add(new ParameterDefinition(_typeReferenceProvider.ObjectArray));
            return logTraceEnterMethod;
        }

        public MethodReference GetTraceLeaveReference()
        {
            var logTraceLeaveMethod = new MethodReference("TraceLeave", _moduleDefinition.TypeSystem.Void, _typeReferenceProvider.LogAdapterReference);
            logTraceLeaveMethod.HasThis = true; //instance method
            logTraceLeaveMethod.Parameters.Add(new ParameterDefinition(_moduleDefinition.TypeSystem.String));
            logTraceLeaveMethod.Parameters.Add(new ParameterDefinition(_moduleDefinition.TypeSystem.Int64));
            logTraceLeaveMethod.Parameters.Add(new ParameterDefinition(_moduleDefinition.TypeSystem.Int64));
            logTraceLeaveMethod.Parameters.Add(new ParameterDefinition(_typeReferenceProvider.StringArray));
            logTraceLeaveMethod.Parameters.Add(new ParameterDefinition(_typeReferenceProvider.ObjectArray));
            return logTraceLeaveMethod;
        }

        public MethodReference GetGetTypeFromHandleReference()
        {
            return _moduleDefinition.ImportReference(typeof(Type).GetRuntimeMethod("GetTypeFromHandle", new [] { typeof(RuntimeTypeHandle)}));
        }

        public MethodReference GetTimestampReference()
        {
            return _moduleDefinition.ImportReference(typeof(Stopwatch).GetRuntimeMethod("GetTimestamp", new Type[0]));
        }

        public MethodReference GetInstanceLogMethod(MethodReferenceInfo methodReferenceInfo, IEnumerable<ParameterDefinition> parameters = null)
        {
            parameters = parameters ?? new ParameterDefinition[0];

            var logMethod = new MethodReference(GetInstanceLogMethodName(methodReferenceInfo), methodReferenceInfo.ReturnType, _typeReferenceProvider.LogAdapterReference);
            logMethod.HasThis = true; //instance method

            //check if accessor
            if (!methodReferenceInfo.IsPropertyAccessor())
            {
                logMethod.Parameters.Add(new ParameterDefinition(_moduleDefinition.TypeSystem.String));
            }

            foreach (var parameter in parameters)
            {
                logMethod.Parameters.Add(parameter);
            }

            //handle generics
            if (methodReferenceInfo.IsGeneric)
            {
                foreach (var genericParameter in methodReferenceInfo.GenericParameters)
                {
                    var gp = new GenericParameter(genericParameter.Name, logMethod);
                    gp.Name = genericParameter.Name;
                    logMethod.GenericParameters.Add(gp);
                }
                logMethod.CallingConvention = MethodCallingConvention.Generic;

                logMethod = new GenericInstanceMethod(logMethod);
                foreach (var genericArgument in methodReferenceInfo.GenericArguments)
                {
                    ((GenericInstanceMethod)logMethod).GenericArguments.Add(genericArgument);
                }
            }

            return logMethod;
        }

        private string GetInstanceLogMethodName(MethodReferenceInfo methodReferenceInfo)
        {
            //TODO chain inner types in name
            var typeName = methodReferenceInfo.DeclaringType.Name;

            if (methodReferenceInfo.IsPropertyAccessor())
            {
                return "get_" + typeName + methodReferenceInfo.Name.Substring(4);
            }
            else
            {
                return typeName + methodReferenceInfo.Name;
            }
        }

    }
}
