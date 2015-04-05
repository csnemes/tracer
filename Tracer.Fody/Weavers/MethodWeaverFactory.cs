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
        private readonly MethodWeaver.ILoggerProvider _loggerProvider;

        public MethodWeaverFactory(TypeReferenceProvider typeReferenceProvider, MethodReferenceProvider methodReferenceProvider,
            MethodWeaver.ILoggerProvider loggerProvider)
        {
            _typeReferenceProvider = typeReferenceProvider;
            _methodReferenceProvider = methodReferenceProvider;
            _loggerProvider = loggerProvider;
        }

        public MethodWeaver Create(MethodDefinition methodDefinition)
        {
            return new MethodWeaver(_typeReferenceProvider, _methodReferenceProvider, _loggerProvider, methodDefinition);
        }
    }
}
