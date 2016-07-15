using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Mono.Cecil;

namespace Tracer.Fody.Filters
{
    /// <summary>
    /// Base class for tracer xml configuration 
    /// </summary>
    internal abstract class AssemblyLevelTraceDefinition
    {
        private readonly NamespaceScope _namespace;

        protected AssemblyLevelTraceDefinition(NamespaceScope ns)
        {
            _namespace = ns;
        }
        
        internal NamespaceScope NamespaceScope
        {
            get { return _namespace; }
        }
        
        protected static NamespaceScope ParseNamespaceScope(XElement element)
        {
            var attribute = element.Attribute("namespace");
            if (attribute == null) return NamespaceScope.All;
            try
            {
                return NamespaceScope.Parse(attribute.Value);
            }
            catch (Exception ex)
            {
                throw new ApplicationException(String.Format("Failed to parse configuration line {0}. See inner exception for details.", element.ToString()), ex);
            }
        }

        internal virtual bool IsMatching(MethodDefinition methodDefinition) //TypeDefinition declaringType, VisibilityLevel methodVisibilityLevel)
        {
            return _namespace.IsMatching(methodDefinition.DeclaringType.Namespace);
        }

        internal abstract bool ShouldTrace();

    }
}
