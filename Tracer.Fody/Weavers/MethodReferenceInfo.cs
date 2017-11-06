using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Mono.Collections.Generic;
using Tracer.Fody.Helpers;

namespace Tracer.Fody.Weavers
{
    /// <summary>
    /// Class that represents a method used for weaving. It starts with MethodReference but can resolve it to MethodDeclaration. In some (weird) cases resolving does not work.
    /// In such scenarios it falls back to data present in the MethodReference
    /// </summary>
    internal class MethodReferenceInfo
    {
        private readonly MethodReference _methodReference;
        private readonly MethodDefinition _methodDefinition;

        public MethodReferenceInfo(MethodReference methodReference)
        {
            _methodReference = methodReference;
            _methodDefinition = methodReference.Resolve();
        }

        public string Name
        {
            get { return _methodReference.Name; }
        }

        public TypeReference DeclaringType
        {
            get { return _methodReference.DeclaringType; }
        }

        public bool IsPropertyAccessor()
        {
            if (_methodDefinition != null) return _methodDefinition.IsPropertyAccessor();

            //lame fallback 
            return _methodReference.Name.StartsWith("get_", StringComparison.OrdinalIgnoreCase) || _methodReference.Name.StartsWith("set_", StringComparison.OrdinalIgnoreCase);
        }

        public TypeReference ReturnType
        {
            get { return _methodReference.ReturnType; }
        }

        public bool IsSetter
        {
            get
            {
                if (_methodDefinition != null) return _methodDefinition.IsSetter;

                //lame fallback 
                return _methodReference.Name.StartsWith("set_", StringComparison.OrdinalIgnoreCase);
            }
        }

        public bool IsGeneric
        {
            get
            {
                if (_methodDefinition != null) return _methodDefinition.HasGenericParameters;
                return _methodReference.HasGenericParameters;
            }
        }

        public Collection<GenericParameter> GenericParameters
        {
            get
            {
                if (_methodDefinition != null) return _methodDefinition.GenericParameters;
                return _methodReference.GenericParameters;
            }
        }

        public Collection<TypeReference> GenericArguments
        {
            get
            {
                var genericMethodRef = _methodReference as GenericInstanceMethod;
                if (genericMethodRef == null) return new Collection<TypeReference>();
                return genericMethodRef.GenericArguments;
            }
        }
    }
}
