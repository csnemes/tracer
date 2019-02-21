using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Mono.Cecil;
using Tracer.Fody.Weavers;

namespace Tracer.Fody.Filters.DefaultFilter
{
    /// <summary>
    /// The default filter used for weaving. It processes information from the FodyWeavers.xml belonging to Tracer to 
    /// identify which methods/classes the weaver needs to trace.
    /// </summary>
    internal class DefaultFilter : ITraceLoggingFilter
    {
        private readonly List<AssemblyLevelTraceDefinition> _assemblyLevelTraceDefinitions;
        private readonly TraceAttributeHelper _traceAttributeHelper = new TraceAttributeHelper();

        private static readonly List<AssemblyLevelTraceOnDefinition> DefaultAssemblyLevelTraceDefinitions = new List
            <AssemblyLevelTraceOnDefinition> { new AssemblyLevelTraceOnDefinition(NamespaceScope.All, TraceTargetVisibility.Public, TraceTargetVisibility.Public) };

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

            return _traceAttributeHelper.ShouldTraceBasedOnMethodLevelInfo(definition) ??
                   _traceAttributeHelper.ShouldTraceBasedOnClassLevelInfo(definition) ?? 
                   ShouldTraceBasedOnAssemblyLevelInfo(definition);
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

    }

}
