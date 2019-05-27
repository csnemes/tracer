using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Mono.Cecil;

namespace Tracer.Fody.Filters.PatternFilter
{
    public class PatternDefinition : IComparable<PatternDefinition>
    {
        private readonly bool _traceEnabled;
        private readonly Dictionary<string, string> _parameters;
        private readonly NamespaceMatcher _namespaceMatcher;
        private readonly ClassMatcher _classMatcher;
        private readonly MemberMatcher _memberMatcher;
        private readonly IMatcher<string> _cachedNamespaceMatcher;
        private readonly IMatcher<TypeDefinition> _cachedClassMatcher;
        private readonly string _originalExpression;

        private PatternDefinition(bool traceEnabled, NamespaceMatcher namespaceMatcher, ClassMatcher classMatcher, MemberMatcher memberMatcher, Dictionary<string, string> parameters, string originalExpression)
        {
            _traceEnabled = traceEnabled;
            _namespaceMatcher = namespaceMatcher;
            _classMatcher = classMatcher;
            _memberMatcher = memberMatcher;
            _originalExpression = originalExpression;
            _cachedNamespaceMatcher = new CachingDecorator<string>(namespaceMatcher);
            _cachedClassMatcher = new CachingDecorator<TypeDefinition>(classMatcher);
            _parameters = parameters ?? new Dictionary<string, string>();
        }
        public override string ToString() => _originalExpression;

        public bool TraceEnabled => _traceEnabled;

        public Dictionary<string, string> Parameters => _parameters;

        internal static PatternDefinition ParseFromConfig(XElement element, bool traceEnabled)
        {
            var pattern = element.Attribute("pattern")?.Value;
            if (pattern == null) throw new Exception($"Pattern is missing from configuration line: {element.Value}.");

            var parameters = element.Attributes().Where(it => it.Name.LocalName != "pattern")
                .ToDictionary(it => it.Name.LocalName, it => it.Value);

            return BuildUpDefinition(pattern, traceEnabled, parameters);
        }

        internal static PatternDefinition BuildUpDefinition(string pattern, bool traceEnabled, Dictionary<string, string> parameters = null)
        {
            if (pattern == "*" || pattern == "*.*" || pattern == ".." ) pattern = "..*.*";

            var memberSeparatorIdx = pattern.LastIndexOf('.');
            if (memberSeparatorIdx <= 0)
                throw new Exception($"Invalid pattern format {pattern}");

            var classSeparatorIdx = pattern.LastIndexOf('.', memberSeparatorIdx - 1);
            if (classSeparatorIdx <= 0)
                throw new Exception($"Invalid pattern format {pattern}");



            var memberPart = pattern.Substring(memberSeparatorIdx + 1);
            var classPart = pattern.Substring(classSeparatorIdx + 1, memberSeparatorIdx - classSeparatorIdx - 1);
            var nameSpacePart = pattern.Substring(0, classSeparatorIdx);

            //fixing weird dottings
            if (nameSpacePart.Length > 2)
            {
                if (nameSpacePart[0] == '.' && nameSpacePart[1] != '.') nameSpacePart = "." + nameSpacePart;
                if (nameSpacePart[nameSpacePart.Length-1] == '.' && nameSpacePart[nameSpacePart.Length-2] != '.') nameSpacePart = nameSpacePart + ".";
            }
            else
            {
                if (nameSpacePart[0] == '.') nameSpacePart = "." + nameSpacePart;
            }

            return new PatternDefinition(traceEnabled, new NamespaceMatcher(nameSpacePart), ClassMatcher.Create(classPart), MemberMatcher.Create(memberPart),
                parameters, pattern);
        }

        public bool IsMatching(MethodDefinition methodDefinition)
        {
            var ns = methodDefinition.DeclaringType.Namespace;
            return _cachedNamespaceMatcher.IsMatch(ns) && _cachedClassMatcher.IsMatch(methodDefinition.DeclaringType) &&
                _memberMatcher.IsMatch(methodDefinition);
        }

        public int CompareTo(PatternDefinition other)
        {
            if (_memberMatcher.IsAllFilter && !other._memberMatcher.IsAllFilter) return 1;
            if (!_memberMatcher.IsAllFilter && other._memberMatcher.IsAllFilter) return -1;

            var nsMatch = this._namespaceMatcher?.CompareTo(other?._namespaceMatcher) ?? 0;
            if (nsMatch != 0)
            {
                if (_classMatcher.IsAllFilter && !other._classMatcher.IsAllFilter) return 1;
                if (!_classMatcher.IsAllFilter && other._classMatcher.IsAllFilter) return -1;
                return nsMatch;
            }

            var classMatch = this._classMatcher?.CompareTo(other?._classMatcher) ?? 0;
            if (classMatch != 0)
            {
                if (_memberMatcher.IsAllFilter && !other._memberMatcher.IsAllFilter) return 1;
                if (!_memberMatcher.IsAllFilter && other._memberMatcher.IsAllFilter) return -1;
                return classMatch;
            }

            return this?._memberMatcher?.CompareTo(other?._memberMatcher) ?? 0;
        }
    }
}
