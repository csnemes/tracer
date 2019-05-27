using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Mono.Cecil;

namespace Tracer.Fody.Filters.PatternFilter
{
    public class MemberMatcher : IComparable<MemberMatcher>, IMatcher<MethodDefinition>
    {
        private readonly Regex _regex;
        private readonly bool _matchPublic;
        private readonly bool _matchPrivate;
        private readonly bool _matchInternal;
        private readonly bool _matchProtected;
        private readonly bool _matchInstance;
        private readonly bool _matchStatic;
        private readonly bool _matchMethod;
        private readonly bool _matchConstructor;
        private readonly bool _matchPropertySet;
        private readonly bool _matchPropertyGet;
        private readonly string _filterExpression;
        private readonly int _conditionCount;

        private static readonly string[] Keywords = { "public", "private", "internal", "protected", "instance", "static", "method", "get", "set", "constructor" };

        private MemberMatcher(string regexPattern, bool matchPublic, bool matchPrivate, bool matchInternal, bool matchProtected,
            bool matchInstance, bool matchStatic, bool matchMethod, bool matchPropertySet, bool matchPropertyGet, bool matchConstructor, string filterExpression, int conditionCount)
        {
            _regex = new Regex(regexPattern,
                RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant);

            _matchPublic = matchPublic;
            _matchPrivate = matchPrivate;
            _matchInternal = matchInternal;
            _matchProtected = matchProtected;

            if (!matchPrivate && !matchPublic && !matchInternal && !matchProtected)
            {
                _matchPrivate = _matchPublic = _matchInternal = _matchProtected = true;
            }

            _matchInstance = matchInstance;
            _matchStatic = matchStatic;

            if (!matchInstance && !matchStatic)
            {
                _matchInstance = _matchStatic = true;
            }

            _matchMethod = matchMethod;
            _matchPropertySet = matchPropertySet;
            _matchPropertyGet = matchPropertyGet;
            _matchConstructor = matchConstructor;

            if (!matchMethod && !matchPropertySet && !matchPropertyGet &&!matchConstructor)
            {
                _matchMethod = _matchPropertySet = _matchPropertyGet = _matchConstructor = true;
            }

            _filterExpression = filterExpression;
            _conditionCount = conditionCount;
        }

        public bool IsMatch(MethodDefinition methodDefinition)
        {
            if (methodDefinition.IsConstructor)
            {
                //name doesn't matter
                if (methodDefinition.IsStatic)
                {
                    return _matchConstructor && _matchStatic;
                }
                else
                {
                    return _matchConstructor && _matchInstance && CheckVisibility();
                }
            }

            var methodName = (methodDefinition.IsSetter || methodDefinition.IsGetter) ? methodDefinition.Name.Substring(4) :
                methodDefinition.Name;

            if (methodDefinition.HasGenericParameters)
            {
                var backtickIndex = methodName.IndexOf('`');
                if (backtickIndex > 0) methodName = methodName.Substring(0, backtickIndex);
            }

            return _regex.IsMatch(methodName) && CheckVisibility() && CheckStaticOrInstance() && CheckMemberType();

            bool CheckStaticOrInstance()
            {
                if (methodDefinition.IsStatic) return _matchStatic;
                return _matchInstance;
            }

            bool CheckVisibility()
            {
                if (methodDefinition.IsPublic) return _matchPublic;
                if (methodDefinition.IsPrivate) return _matchPrivate;
                if (methodDefinition.IsFamily || methodDefinition.IsFamilyOrAssembly || methodDefinition.IsFamilyAndAssembly) return _matchProtected;
                if (methodDefinition.IsAssembly || methodDefinition.IsFamilyOrAssembly || methodDefinition.IsFamilyAndAssembly) return _matchInternal;
                return true;
            }

            bool CheckMemberType()
            {
                if (methodDefinition.IsGetter) return _matchPropertyGet;
                if (methodDefinition.IsSetter) return _matchPropertySet;
                return _matchMethod;
            }
        }

        public static MemberMatcher Create(string input)
        {
            var filterExpression = input;

            if (string.IsNullOrWhiteSpace(filterExpression))
                throw new ArgumentException("Filter expression's method name part cannot be empty.", nameof(filterExpression));

            var conditions = new List<string>();

            if (filterExpression.StartsWith("["))
            {
                var closingPosition = filterExpression.IndexOf(']');
                if (closingPosition == -1)
                    throw new ArgumentException("Missing closing square bracket in filter expression's method name part.",
                        nameof(filterExpression));

                var conditionsExpression = filterExpression.Substring(1, closingPosition - 1);
                filterExpression = filterExpression.Substring(closingPosition + 1);
                conditions = conditionsExpression.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(it => it.Trim().ToLowerInvariant())
                    .ToList();
            }

            if (filterExpression.Contains("]"))
                throw new ArgumentException("Invalid closing square bracket in filter expression's method name part.",
                    nameof(filterExpression));

            if (string.IsNullOrWhiteSpace(filterExpression))
                throw new ArgumentException("Filter expression's method name part cannot be empty.",
                    nameof(filterExpression));

            var badKeyword = conditions.SkipWhile(it => Keywords.Contains(it)).FirstOrDefault();
            if (badKeyword != null)
                throw new ArgumentException($"Keyword {badKeyword} is not recognized in {input}");

            var regexPattern = filterExpression.Replace("?", "[a-z0-9_]").Replace("*", "[a-z0-9_]*");

            regexPattern = "^" + regexPattern + "$";

            return new MemberMatcher(regexPattern,
                conditions.Contains("public"),
                conditions.Contains("private"),
                conditions.Contains("internal"),
                conditions.Contains("protected"),
                conditions.Contains("instance"),
                conditions.Contains("static"),
                conditions.Contains("method"),
                conditions.Contains("get"),
                conditions.Contains("set"),
                conditions.Contains("constructor"),
                filterExpression,
                conditions.Count);
        }

        public int CompareTo(MemberMatcher other)
        {
            var otherQmark = other._filterExpression.Count(it => it == '?');
            var otherStar = other._filterExpression.Count(it => it == '*');

            var thisQmark = this._filterExpression.Count(it => it == '?');
            var thisStar = this._filterExpression.Count(it => it == '*');

            if (otherStar > 0 && thisStar == 0) return -1; //this precedes other
            if (thisStar > 0 && otherStar == 0) return 1; //other precedes this
            if (thisStar == 0 && otherStar == 0)
            {
                var qmarkDiff = thisQmark - otherQmark;
                if (qmarkDiff != 0) return qmarkDiff;
                return other._conditionCount - _conditionCount;
            }

            var defDiff = (other._filterExpression.Length - otherStar - otherQmark) - (this._filterExpression.Length - thisStar - thisQmark);
            if (defDiff != 0) return defDiff;
            return other._conditionCount - _conditionCount;
        }
    }
}
