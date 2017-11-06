using System;
using System.Xml.Linq;
using Mono.Cecil;

namespace Tracer.Fody.Filters
{
    /// <summary>
    /// Specifies that the namespace should be traced for the given class and method visibility level.
    /// </summary>
    internal class AssemblyLevelTraceOnDefinition : AssemblyLevelTraceDefinition
    {
        private readonly TraceTargetVisibility _targetClass;
        private readonly TraceTargetVisibility _targetMethod;

        internal AssemblyLevelTraceOnDefinition(NamespaceScope namespc, TraceTargetVisibility targetClass, TraceTargetVisibility targetMethod) : base(namespc)
        {
            _targetClass = targetClass;
            _targetMethod = targetMethod;
        }

        internal static AssemblyLevelTraceOnDefinition ParseFromConfig(XElement element)
        {
            return new AssemblyLevelTraceOnDefinition(ParseNamespaceScope(element), ParseTraceTargetVisibility(element, "class"), ParseTraceTargetVisibility(element, "method"));
        }

        private static TraceTargetVisibility ParseTraceTargetVisibility(XElement element, string attributeName)
        {
            var attribute = element.Attribute(attributeName);
            if (attribute == null)
            {
                throw new Exception(String.Format("Tracer: TraceOn config element missing attribute '{0}'.", attributeName));
            }

            switch (attribute.Value.ToLower())
            {
                case "public": return TraceTargetVisibility.Public;
                case "internal": return TraceTargetVisibility.InternalOrMoreVisible;
                case "protected": return TraceTargetVisibility.ProtectedOrMoreVisible;
                case "private": return TraceTargetVisibility.All;
                case "none": return TraceTargetVisibility.None;
                default:
                    throw new Exception(String.Format("Tracer: TraceOn config element attribute '{0}' has an invalid value '{1}'.", attributeName, attribute.Value));
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

        internal override bool IsMatching(MethodDefinition methodDefinition)
        {
            if (base.IsMatching(methodDefinition))
            {
                var declaringType = methodDefinition.DeclaringType;
                var typeVisibility = VisibilityHelper.GetTypeVisibilityLevel(declaringType);
                var methodVisibilityLevel = VisibilityHelper.GetMethodVisibilityLevel(methodDefinition);

                //check type visibility
                if ((int)typeVisibility > (int)_targetClass) return false;

                //then method visibility will decide
                return ((int)methodVisibilityLevel <= (int)_targetMethod);
            }
            return false;
        }


        protected bool Equals(AssemblyLevelTraceOnDefinition other)
        {
            return _targetClass == other._targetClass && _targetMethod == other._targetMethod && NamespaceScope == other.NamespaceScope;
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

        internal override bool ShouldTrace()
        {
            return true;
        }
    }
}