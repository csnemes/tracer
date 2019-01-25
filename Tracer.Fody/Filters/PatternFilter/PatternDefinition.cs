using System;
using System.Xml.Linq;
using Mono.Cecil;

namespace Tracer.Fody.Filters.PatternFilter
{
    public class PatternDefinition
    {
        private readonly bool _traceEnabled;
        private readonly NamespaceMatcher _namespaceMatcher;
        private readonly ClassMatcher _classMatcher;
        private readonly MemberMatcher _memberMatcher;

        private PatternDefinition(bool traceEnabled, NamespaceMatcher namespaceMatcher, ClassMatcher classMatcher, MemberMatcher memberMatcher)
        {
            _traceEnabled = traceEnabled;
            _namespaceMatcher = namespaceMatcher;
            _classMatcher = classMatcher;
            _memberMatcher = memberMatcher;
        }

        public bool TraceEnabled => _traceEnabled;

        internal static PatternDefinition ParseFromConfig(XElement element, bool traceEnabled)
        {
            var pattern = element.Attribute("pattern")?.Value;
            if (pattern == null) throw new Exception($"Pattern is missing from configuration line: {element.Value}.");
            return BuildUpDefinition(pattern, traceEnabled);
        }

        internal static PatternDefinition BuildUpDefinition(string pattern, bool traceEnabled)
        {
            var memberSeparatorIdx = pattern.LastIndexOf('.');
            if (memberSeparatorIdx <= 0)
                throw new Exception($"Invalid pattern format {pattern}");

            var classSeparatorIdx = pattern.LastIndexOf('.', memberSeparatorIdx - 1);
            if (classSeparatorIdx <= 0)
                throw new Exception($"Invalid pattern format {pattern}");



            var memberPart = pattern.Substring(memberSeparatorIdx + 1);
            var classPart = pattern.Substring(classSeparatorIdx, memberSeparatorIdx - classSeparatorIdx);
            var nameSpacePart = pattern.Substring(0, classSeparatorIdx);

            return new PatternDefinition(traceEnabled, new NamespaceMatcher(nameSpacePart), ClassMatcher.Create(classPart), MemberMatcher.Create(memberPart));
        }

        public bool IsMatching(MethodDefinition methodDefinition)
        {
            var ns = methodDefinition.DeclaringType.Namespace;
            return _namespaceMatcher.IsMatch(ns) && _classMatcher.IsMatch(methodDefinition.DeclaringType) &&
                _memberMatcher.IsMatch(methodDefinition);
        }
    }
}
