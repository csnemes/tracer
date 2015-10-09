using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Mono.Cecil;
using Tracer.Fody.Weavers;

namespace Tracer.Fody.Filters
{
    internal class DefaultFilter : ITraceLoggingFilter
    {
        private readonly List<AssemblyLevelTraceDefinition> _assemblyLevelTraceDefinitions;

        private static readonly List<AssemblyLevelTraceDefinition> DefaultAssemblyLevelTraceDefinitions = new List
            <AssemblyLevelTraceDefinition> { new AssemblyLevelTraceDefinition(TraceTargetVisibility.Public, TraceTargetVisibility.Public) };

        private readonly Dictionary<string, TraceAttributeInfo> _traceAttributeCache = new Dictionary<string, TraceAttributeInfo>();

        public DefaultFilter(IEnumerable<XElement> configElements) : this(ParseConfig(configElements)) 
        {}

        public DefaultFilter(IEnumerable<AssemblyLevelTraceDefinition> configDefinitions)
        {
            //sort from most specific to least specific
            _assemblyLevelTraceDefinitions = (configDefinitions.Any() ? configDefinitions : DefaultAssemblyLevelTraceDefinitions)
                .OrderBy(def => (int)def.TargetClass).ToList();
        }

        internal static IEnumerable<AssemblyLevelTraceDefinition> ParseConfig(IEnumerable<XElement> configElements)
        {
            return configElements.Where(elem => elem.Name.LocalName.Equals("TraceOn", StringComparison.OrdinalIgnoreCase))
                    .Select(AssemblyLevelTraceDefinition.ParseFromConfig).ToList();
        }

        public bool ShouldAddTrace(MethodDefinition definition)
        {
            //Trace attribute defined closer to the method overrides more generic definitions
            //So the check order is method -> class -> outer class -> assembly level specs

            return ShouldTraceBasedOnMethodLevelInfo(definition) ??
                   ShouldTraceBasedOnClassLevelInfo(definition) ?? 
                   ShouldTraceBasedOnAssemblyLevelInfo(definition);
        }
        
        private bool? ShouldTraceBasedOnMethodLevelInfo(MethodDefinition definition)
        {
            if (definition.CustomAttributes.Any(attr => attr.AttributeType.FullName.Equals("TracerAttributes.TraceOn", StringComparison.Ordinal)))
                return true;
            if (definition.CustomAttributes.Any(attr => attr.AttributeType.FullName.Equals("TracerAttributes.NoTrace", StringComparison.Ordinal)))
                return false;
            return null;
        }

        private bool? ShouldTraceBasedOnClassLevelInfo(MethodDefinition definition)
        {
            var attributeInfo = GetTraceAttributeInfoForType(definition.DeclaringType);

            if (attributeInfo != null)
            {
                if (attributeInfo.IsNoTrace) { return false; }

                var targetVisibility = attributeInfo.TargetVisibility;
                var methodVisibility = GetMethodVisibilityLevel(definition);
                return ((int)targetVisibility >= (int)methodVisibility);

            }

            return null;
        }

        private TraceAttributeInfo GetTraceAttributeInfoForType(TypeDefinition type)
        {
            TraceAttributeInfo result = null;
            if (!_traceAttributeCache.TryGetValue(type.FullName, out result))
            {
                result = GetNearestTraceAttributeWalkingUpTheTypeNestingChain(type);
                _traceAttributeCache.Add(type.FullName, result);
            }

            return result;
        }

        private TraceAttributeInfo GetNearestTraceAttributeWalkingUpTheTypeNestingChain(TypeDefinition type)
        {
            //with NoTrace present skip tracing
            if (type.CustomAttributes.Any(
                attr => attr.AttributeType.FullName.Equals("TracerAttributes.NoTrace", StringComparison.Ordinal)))
            {
                return TraceAttributeInfo.NoTrace();
            }

            var traceOnAttribute = type.CustomAttributes.FirstOrDefault(
                attr => attr.AttributeType.FullName.Equals("TracerAttributes.TraceOn", StringComparison.Ordinal));

            if (traceOnAttribute != null)
            {
                return TraceAttributeInfo.TraceOn(GetTargetVisibilityFromAttribute(traceOnAttribute));
            }

            //no attrib present on type see if we have an outer class
            if (type.DeclaringType != null) return GetNearestTraceAttributeWalkingUpTheTypeNestingChain(type.DeclaringType);

            return null;
        }

        private TraceTargetVisibility GetTargetVisibilityFromAttribute(CustomAttribute attribute)
        {
            var intVal = 0; //defaults to public
            if (attribute.HasProperties)
            {
                intVal = (int)attribute.Properties[0].Argument.Value;
            }
            else if (attribute.HasConstructorArguments)
            {
                intVal = (int) attribute.ConstructorArguments[0].Value;
            }

            return (TraceTargetVisibility) intVal;
        }

        private bool ShouldTraceBasedOnAssemblyLevelInfo(MethodDefinition definition)
        {
            var declaringType = definition.DeclaringType;
            
            //get matching assembly level rule (note that defs are ordered from public to private)
            var rule = _assemblyLevelTraceDefinitions.FirstOrDefault(
                def => (int)def.TargetClass >= (int)GetTypeVisibilityLevel(declaringType));

            if (rule != null)
            {
                var methodVisibility = GetMethodVisibilityLevel(definition);
                return ((int)rule.TargetMethod >= (int)methodVisibility);
            }

            return false;
        }

        private VisibilityLevel GetTypeVisibilityLevel(TypeDefinition typeDefinition)
        {
            if (typeDefinition.IsNested)
            {
                if (typeDefinition.IsNestedPublic) return VisibilityLevel.Public;
                if (typeDefinition.IsNestedAssembly) return VisibilityLevel.Internal;
                if (typeDefinition.IsNestedFamilyOrAssembly) return VisibilityLevel.Internal; //protected internal
                if (typeDefinition.IsNestedFamily) return VisibilityLevel.Protected;
                return VisibilityLevel.Private;
            }
            if (typeDefinition.IsPublic) return VisibilityLevel.Public;
            return VisibilityLevel.Internal;
        }

        private VisibilityLevel GetMethodVisibilityLevel(MethodDefinition methodDefinition)
        {
            if (methodDefinition.IsPublic) return VisibilityLevel.Public;
            if (methodDefinition.IsAssembly) return VisibilityLevel.Internal;
            if (methodDefinition.IsFamilyOrAssembly) return VisibilityLevel.Internal;
            if (methodDefinition.IsFamily) return VisibilityLevel.Protected;
            return VisibilityLevel.Private;
        }



        public enum TraceTargetVisibility
        {
            None = -1,
            Public = 0,
            InternalOrMoreVisible = 1,
            ProtectedOrMoreVisible = 2,
            All = 3
        }

        private enum VisibilityLevel
        {
            Public = 0,
            Internal = 1,
            Protected = 2,
            Private = 3
        }

        internal class AssemblyLevelTraceDefinition
        {
            private readonly TraceTargetVisibility _targetClass;
            private readonly TraceTargetVisibility _targetMethod;

            internal AssemblyLevelTraceDefinition(TraceTargetVisibility targetClass, TraceTargetVisibility targetMethod)
            {
                _targetClass = targetClass;
                _targetMethod = targetMethod;
            }

            internal static AssemblyLevelTraceDefinition ParseFromConfig(XElement element)
            {
                return new AssemblyLevelTraceDefinition(ParseTraceTargetVisibility(element, "class"), ParseTraceTargetVisibility(element, "method"));
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

            protected bool Equals(AssemblyLevelTraceDefinition other)
            {
                return _targetClass == other._targetClass && _targetMethod == other._targetMethod;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((AssemblyLevelTraceDefinition) obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return ((int) _targetClass*397) ^ (int) _targetMethod;
                }
            }

            public static bool operator ==(AssemblyLevelTraceDefinition left, AssemblyLevelTraceDefinition right)
            {
                return Equals(left, right);
            }

            public static bool operator !=(AssemblyLevelTraceDefinition left, AssemblyLevelTraceDefinition right)
            {
                return !Equals(left, right);
            }
        }

        private class TraceAttributeInfo
        {
            private readonly TraceTargetVisibility _targetVisibility;
            private readonly bool _noTrace;

            private TraceAttributeInfo(TraceTargetVisibility targetVisibility, bool noTrace)
            {
                _targetVisibility = targetVisibility;
                _noTrace = noTrace;
            }

            public static TraceAttributeInfo NoTrace()
            {
                return new TraceAttributeInfo(TraceTargetVisibility.All, true);
            }

            public static TraceAttributeInfo TraceOn(TraceTargetVisibility visibility)
            {
                return new TraceAttributeInfo(visibility, false);
            }

            public bool IsNoTrace
            {
                get { return _noTrace; }
            }

            public bool IsTraceOn
            {
                get { return !_noTrace; }
            }

            public TraceTargetVisibility TargetVisibility
            {
                get
                {
                    if (!IsTraceOn) throw new ApplicationException("Not a traceOn result.");
                    return _targetVisibility;
                }
            }
        }
    }
}
