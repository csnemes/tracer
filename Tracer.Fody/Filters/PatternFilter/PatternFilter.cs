using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Mono.Cecil;
using Tracer.Fody.Weavers;

namespace Tracer.Fody.Filters.PatternFilter
{
    public class PatternFilter : ITraceLoggingFilter
    {
        private readonly List<PatternDefinition> _patternDefinitions;

        public PatternFilter(IEnumerable<XElement> configElements) : this(ParseConfig(configElements))
        { }

        public PatternFilter(List<PatternDefinition> patternDefinitions)
        {
            _patternDefinitions = patternDefinitions;
        }

        public bool ShouldAddTrace(MethodDefinition definition)
        {
            foreach (var patternDefinition in _patternDefinitions)
            {
                if (patternDefinition.IsMatching(definition)) return patternDefinition.TraceEnabled;
            }

            return false;
        }

        internal static List<PatternDefinition> ParseConfig(IEnumerable<XElement> configElements)
        {
            return configElements
                .Where(elem => elem.Name.LocalName.Equals("TraceOn", StringComparison.OrdinalIgnoreCase))
                .Select(it => PatternDefinition.ParseFromConfig(it, true))
                .Concat(configElements
                    .Where(elem => elem.Name.LocalName.Equals("TraceOff", StringComparison.OrdinalIgnoreCase))
                    .Select(it => PatternDefinition.ParseFromConfig(it, false))).ToList();
        }
    }
}
