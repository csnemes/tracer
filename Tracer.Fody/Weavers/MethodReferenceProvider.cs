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
            return _moduleDefinition.ImportReference(typeof(Type).GetMethod("GetTypeFromHandle", BindingFlags.Public | BindingFlags.Static));
        }

        public MethodReference GetTimestampReference()
        {
            return _moduleDefinition.ImportReference(typeof(Stopwatch).GetMethod("GetTimestamp", BindingFlags.Public | BindingFlags.Static));
        }

        public MethodReference GetInstanceLogMethodWithoutParameter(MethodDefinition methodDefinition)
        {
            var logMethod = new MethodReference(GetInstanceLogMethodName(methodDefinition), methodDefinition.ReturnType, _typeReferenceProvider.LogAdapterReference);
            logMethod.HasThis = true; //instance method

            if (!methodDefinition.IsPropertyAccessor())
            {
                logMethod.Parameters.Add(new ParameterDefinition(_moduleDefinition.TypeSystem.String));
            }

            return logMethod;
        }

        private string GetInstanceLogMethodName(MethodDefinition methodDefinition)
        {
            //TODO chain inner types in name
            var typeName = methodDefinition.DeclaringType.Name;

            if (methodDefinition.IsPropertyAccessor())
            {
                return "get_" + typeName + methodDefinition.Name.Substring(4);
            }
            else
            {
                return typeName + methodDefinition.Name;
            }
        }

        public MethodReference GetInstanceLogMethodWithParameter(MethodDefinition methodDefinition, IEnumerable<ParameterDefinition> parameters)
        {
            var logMethod = new MethodReference(GetInstanceLogMethodName(methodDefinition), methodDefinition.ReturnType, _typeReferenceProvider.LogAdapterReference);
            logMethod.HasThis = true; //instance method
            if (!methodDefinition.IsPropertyAccessor())
            {
                logMethod.Parameters.Add(new ParameterDefinition(_moduleDefinition.TypeSystem.String));
            }

            foreach (var parameter in parameters)
            {
                logMethod.Parameters.Add(parameter);
            }
            return logMethod;
        }
    }
}
