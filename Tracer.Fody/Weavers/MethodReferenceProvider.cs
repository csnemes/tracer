using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;

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

        public MethodReference GetTraceEnterWithParametersReference()
        {
            var logTraceEnterMethod = new MethodReference("TraceEnter", _moduleDefinition.TypeSystem.Void, _typeReferenceProvider.LogAdapterReference);
            logTraceEnterMethod.HasThis = true; //instance method
            logTraceEnterMethod.Parameters.Add(new ParameterDefinition(_moduleDefinition.TypeSystem.String));
            logTraceEnterMethod.Parameters.Add(new ParameterDefinition(_typeReferenceProvider.StringArray));
            logTraceEnterMethod.Parameters.Add(new ParameterDefinition(_typeReferenceProvider.ObjectArray));
            return logTraceEnterMethod;
        }

        public MethodReference GetTraceEnterWithoutParametersReference()
        {
            var logTraceEnterMethod = new MethodReference("TraceEnter", _moduleDefinition.TypeSystem.Void, _typeReferenceProvider.LogAdapterReference);
            logTraceEnterMethod.HasThis = true; //instance method
            logTraceEnterMethod.Parameters.Add(new ParameterDefinition(_moduleDefinition.TypeSystem.String));
            return logTraceEnterMethod;
        }

        public MethodReference GetTraceLeaveWithReturnValueReference()
        {
            var logTraceLeaveMethod = new MethodReference("TraceLeave", _moduleDefinition.TypeSystem.Void, _typeReferenceProvider.LogAdapterReference);
            logTraceLeaveMethod.HasThis = true; //instance method
            logTraceLeaveMethod.Parameters.Add(new ParameterDefinition(_moduleDefinition.TypeSystem.String));
            logTraceLeaveMethod.Parameters.Add(new ParameterDefinition(_moduleDefinition.TypeSystem.Int64));
            logTraceLeaveMethod.Parameters.Add(new ParameterDefinition(_moduleDefinition.TypeSystem.Object));
            return logTraceLeaveMethod;
        }

        public MethodReference GetTraceLeaveWithoutReturnValueReference()
        {
            var logTraceLeaveMethod = new MethodReference("TraceLeave", _moduleDefinition.TypeSystem.Void, _typeReferenceProvider.LogAdapterReference);
            logTraceLeaveMethod.HasThis = true; //instance method
            logTraceLeaveMethod.Parameters.Add(new ParameterDefinition(_moduleDefinition.TypeSystem.String));
            logTraceLeaveMethod.Parameters.Add(new ParameterDefinition(_moduleDefinition.TypeSystem.Int64));
            return logTraceLeaveMethod;
        }

        public MethodReference GetGetTypeFromHandleReference()
        {
            return _moduleDefinition.Import(typeof(Type).GetMethod("GetTypeFromHandle", BindingFlags.Public | BindingFlags.Static));
        }
    }
}
