using System;
using System.Xml.Linq;

namespace Tracer.Fody.Filters
{
    internal class AssemblyLevelTraceOnDefinition : AssemblyLevelDefinition
    {
        private readonly TraceTargetVisibility _targetClass;
        private readonly TraceTargetVisibility _targetMethod;
        private readonly NamespaceScope _namespace;

        internal AssemblyLevelTraceOnDefinition(NamespaceScope namespc, TraceTargetVisibility targetClass, TraceTargetVisibility targetMethod)
        {
            _targetClass = targetClass;
            _targetMethod = targetMethod;
            _namespace = namespc;
        }

        internal static AssemblyLevelTraceOnDefinition ParseFromConfig(XElement element)
        {
            return new AssemblyLevelTraceOnDefinition(ParseNamespaceScope(element), ParseTraceTargetVisibility(element, "class"), ParseTraceTargetVisibility(element, "method"));
        }

        private static NamespaceScope ParseNamespaceScope(XElement element)
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

        private static TraceTargetVisibility ParseTraceTargetVisibility(XElement element, string attributeName)
        {
            var attribute = element.Attribute(attributeName);
            if (attribute == null)
            {
                throw new ApplicationException(String.Format("Tracer: TraceOn config element missing attribute {0}.", attributeName));
            }

            switch (attribute.Value.ToLower())
            {
                case "public": return TraceTargetVisibility.Public;
                case "internal": return TraceTargetVisibility.InternalOrMoreVisible;
                case "protected": return TraceTargetVisibility.ProtectedOrMoreVisible;
                case "private": return TraceTargetVisibility.All;
                case "none": return TraceTargetVisibility.None;
                default:
                    throw new ApplicationException(String.Format("Tracer: TraceOn config element attribute {0} has an invalid value {1}.", attributeName, attribute.Value));
            }
        }

        public TraceTargetVisibility TargetClass
        {
            get { return _targetClass; }
        }

        public TraceTargetVisibility TargetMethod
        {
            get { return _targetMethod; }
        }

        public NamespaceScope NamespaceScope
        {
            get { return _namespace; }
        }

        protected bool Equals(AssemblyLevelTraceOnDefinition other)
        {
            return _targetClass == other._targetClass && _targetMethod == other._targetMethod;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((AssemblyLevelTraceOnDefinition) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int) _targetClass*397) ^ (int) _targetMethod;
            }
        }

        public static bool operator ==(AssemblyLevelTraceOnDefinition left, AssemblyLevelTraceOnDefinition right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(AssemblyLevelTraceOnDefinition left, AssemblyLevelTraceOnDefinition right)
        {
            return !Equals(left, right);
        }
    }
}