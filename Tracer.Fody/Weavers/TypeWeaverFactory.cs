using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;

namespace Tracer.Fody.Weavers
{
    internal class TypeWeaverFactory
    {
        private readonly ITraceLoggingFilter _filter;
        private readonly TypeReferenceProvider _typeReferenceProvider;
        private readonly MethodReferenceProvider _methodReferenceProvider;
        private readonly bool _shouldTraceConstructors;

        public TypeWeaverFactory(ITraceLoggingFilter filter, 
            TypeReferenceProvider typeReferenceProvider, 
            MethodReferenceProvider methodReferenceProvider,
            bool shouldTraceConstructors)
        {
            _filter = filter;
            _typeReferenceProvider = typeReferenceProvider;
            _methodReferenceProvider = methodReferenceProvider;
            _shouldTraceConstructors = shouldTraceConstructors;
        }

        public TypeWeaver Create(TypeDefinition typeDefinition)
        {
            return new TypeWeaver(_filter, _shouldTraceConstructors, _typeReferenceProvider, _methodReferenceProvider, typeDefinition);
        }
    }
}
