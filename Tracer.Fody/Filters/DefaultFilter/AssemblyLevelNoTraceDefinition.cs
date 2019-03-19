using System.Xml.Linq;

namespace Tracer.Fody.Filters.DefaultFilter
{
    /// <summary>
    /// Specifies that the namespace should not be traced
    /// </summary>
    internal class AssemblyLevelNoTraceDefinition : AssemblyLevelTraceDefinition
    {
        internal AssemblyLevelNoTraceDefinition(NamespaceScope ns) : base(ns)
        {}

        internal static AssemblyLevelNoTraceDefinition ParseFromConfig(XElement element)
        {
            return new AssemblyLevelNoTraceDefinition(ParseNamespaceScope(element));
        }

        protected bool Equals(AssemblyLevelNoTraceDefinition other)
        {
            return NamespaceScope == other.NamespaceScope;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((AssemblyLevelNoTraceDefinition) obj);
        }

        public override int GetHashCode()
        {
            return  NamespaceScope.GetHashCode();
        }

        internal override bool ShouldTrace()
        {
            return false;
        }
    }
}
