using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;

namespace Tracer.Fody.Weavers
{
    internal class MethodWeaverFactory
    {
        private readonly TypeReferenceProvider _typeReferenceProvider;
        private readonly MethodReferenceProvider _methodReferenceProvider;
        private readonly MethodWeaverBase.ILoggerProvider _loggerProvider;

        public MethodWeaverFactory(TypeReferenceProvider typeReferenceProvider, MethodReferenceProvider methodReferenceProvider,
            MethodWeaverBase.ILoggerProvider loggerProvider)
        {
            _typeReferenceProvider = typeReferenceProvider;
            _methodReferenceProvider = methodReferenceProvider;
            _loggerProvider = loggerProvider;
        }

        public MethodWeaverBase Create(MethodDefinition methodDefinition)
        {
            if (IsAsyncMethod(methodDefinition))
            {
                return new AsyncMethodWeaver(_typeReferenceProvider, _methodReferenceProvider, _loggerProvider, methodDefinition);
            }
            return new MethodWeaver(_typeReferenceProvider, _methodReferenceProvider, _loggerProvider, methodDefinition);
        }

        private bool IsAsyncMethod(MethodDefinition methodDefinition)
        {
            return
                methodDefinition.CustomAttributes.Any(it => it.AttributeType.FullName.Equals(_typeReferenceProvider.AsyncStateMachineAttribute.FullName));
        }
    }
}
