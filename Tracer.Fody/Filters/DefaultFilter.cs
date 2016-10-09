using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Mono.Cecil;
using Tracer.Fody.Helpers;
using Tracer.Fody.Weavers;

namespace Tracer.Fody.Filters
{
    /// <summary>
    /// The default filter used for weaving. It processes information from the FodyWeavers.xml belonging to Tracer to 
    /// identify which methods/classes the weaver needs to trace.
    /// </summary>
    internal class DefaultFilter : ITraceLoggingFilter
    {
        private readonly List<AssemblyLevelTraceDefinition> _assemblyLevelTraceDefinitions;

        private static readonly List<AssemblyLevelTraceOnDefinition> DefaultAssemblyLevelTraceDefinitions = new List
            <AssemblyLevelTraceOnDefinition> { new AssemblyLevelTraceOnDefinition(NamespaceScope.All, TraceTargetVisibility.Public, TraceTargetVisibility.Public) };

        private readonly Dictionary<string, TraceAttributeInfo> _traceAttributeCache = new Dictionary<string, TraceAttributeInfo>();

        public DefaultFilter(IEnumerable<XElement> configElements) : this(ParseConfig(configElements)) 
        {}

        public DefaultFilter(IEnumerable<AssemblyLevelTraceDefinition> configDefinitions)
        {
            //sort from most specific to least specific
            _assemblyLevelTraceDefinitions = (configDefinitions.Any() ? configDefinitions : DefaultAssemblyLevelTraceDefinitions).ToList();
            _assemblyLevelTraceDefinitions.Sort(AssemblyLevelTraceDefinitionComparer.Instance);
        }

        internal static IEnumerable<AssemblyLevelTraceDefinition> ParseConfig(IEnumerable<XElement> configElements)
        {
            return configElements.Where(elem => elem.Name.LocalName.Equals("TraceOn", StringComparison.OrdinalIgnoreCase))
                    .Select(AssemblyLevelTraceOnDefinition.ParseFromConfig).Cast<AssemblyLevelTraceDefinition>()
                .Concat(configElements.Where(elem => elem.Name.LocalName.Equals("NoTrace", StringComparison.OrdinalIgnoreCase))
                .Select(AssemblyLevelNoTraceDefinition.ParseFromConfig)).ToList();
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
            if (!definition.IsPropertyAccessor())
            {
                if (definition.CustomAttributes.Any(attr => attr.AttributeType.FullName.Equals("TracerAttributes.TraceOn", StringComparison.Ordinal)))
                    return true;
                if (definition.CustomAttributes.Any(attr => attr.AttributeType.FullName.Equals("TracerAttributes.NoTrace", StringComparison.Ordinal)))
                    return false;
            }
            else
            { //its a property accessor check the prop for the attribute
                var correspondingProp =
                    definition.DeclaringType.Properties.FirstOrDefault(prop => prop.GetMethod == definition || prop.SetMethod == definition);
                if (correspondingProp != null)
                {
                    if (correspondingProp.CustomAttributes.Any(attr => attr.AttributeType.FullName.Equals("TracerAttributes.TraceOn", StringComparison.Ordinal)))
                        return true;
                    if (correspondingProp.CustomAttributes.Any(attr => attr.AttributeType.FullName.Equals("TracerAttributes.NoTrace", StringComparison.Ordinal)))
                        return false;
                }
            }

            return null;
        }

        private bool? ShouldTraceBasedOnClassLevelInfo(MethodDefinition definition)
        {
            var attributeInfo = GetTraceAttributeInfoForType(definition.DeclaringType);

            if (attributeInfo != null)
            {
                if (attributeInfo.IsNoTrace) { return false; }

                var targetVisibility = attributeInfo.TargetVisibility;
                var methodVisibility = VisibilityHelper.GetMethodVisibilityLevel(definition);
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
            //get matching assembly level rule (note that defs are ordered from more specific to least specific. On same level noTrace trumps traceOn)
            var rule = _assemblyLevelTraceDefinitions.FirstOrDefault(
                    def => def.IsMatching(definition));

            if (rule != null)
            {
                return rule.ShouldTrace();
            }

            return false;
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
