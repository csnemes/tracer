using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text.RegularExpressions;
using Mono.Cecil;

namespace Tracer.Fody.Filters.PatternFilter
{
    public class ClassMatcher : IComparable<ClassMatcher>
    {
        private readonly Regex _regex;
        private readonly bool _matchPublic;
        private readonly bool _matchNonPublic;
        private readonly string _filterExpression;

        private static readonly string[] Keywords = { "public", "nonpublic", "internal" };

        private ClassMatcher(string regexPattern, bool matchPublic, bool matchNonPublic, string filterExpression)
        {
            _regex = new Regex(regexPattern,
                RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant);

            _matchPublic = matchPublic;
            _matchNonPublic = matchNonPublic;
            _filterExpression = filterExpression;

            if (!matchNonPublic && !matchPublic)
            {
                _matchNonPublic = _matchPublic = true;
            }
        }

        public bool IsMatch(TypeDefinition typeDefinition)
        {
            var className = typeDefinition.Name;

            if (typeDefinition.HasGenericParameters)
            {
                var backtickIndex = className.IndexOf('`');
                className = backtickIndex <= 0 ? className : className.Substring(0, backtickIndex);
            }

            return _regex.IsMatch(className) && CheckVisibility();

            bool CheckVisibility()
            {
                if (typeDefinition.IsPublic || typeDefinition.IsNestedPublic) return _matchPublic;
                return _matchNonPublic;
            }
        }

        public static ClassMatcher Create(string input)
        {
            var filterExpression = input;

            if (string.IsNullOrWhiteSpace(filterExpression))
                throw new ArgumentException("Filter expression's class part cannot be empty.", nameof(filterExpression));

            var conditions = new List<string>();

            if (filterExpression.StartsWith("["))
            {
                var closingPosition = filterExpression.IndexOf(']');
                if (closingPosition == -1)
                    throw new ArgumentException("Missing closing square bracket in filter expression's class part.",
                        nameof(filterExpression));

                var conditionsExpression = filterExpression.Substring(1, closingPosition - 1);
                filterExpression = filterExpression.Substring(closingPosition + 1);
                conditions = conditionsExpression.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(it => it.Trim().ToLowerInvariant())
                    .ToList();
            }

            if (filterExpression.Contains("]"))
                throw new ArgumentException("Invalid closing square bracket in filter expression's class name part.",
                    nameof(filterExpression));

            if (string.IsNullOrWhiteSpace(filterExpression))
                throw new ArgumentException("Filter expression's class name part cannot be empty.",
                    nameof(filterExpression));

            var badKeyword = conditions.SkipWhile(it => Keywords.Contains(it)).FirstOrDefault();
            if (badKeyword != null)
                throw new ArgumentException($"Keyword {badKeyword} is not recognized in {input}");

            var regexPattern = filterExpression.Replace("?", "[a-z0-9_]").Replace("*", "[a-z0-9_]*");
            regexPattern = "^" + regexPattern + "$";

            return new ClassMatcher(regexPattern,
                conditions.Contains("public"),
                conditions.Contains("nonpublic") || conditions.Contains("internal"), filterExpression);
        }

        public int CompareTo(ClassMatcher other)
        {
            var otherQmark = other._filterExpression.Count(it => it == '?');
            var otherStar = other._filterExpression.Count(it => it == '*');

            var thisQmark = this._filterExpression.Count(it => it == '?');
            var thisStar = this._filterExpression.Count(it => it == '*');

            if (otherStar > 0 && thisStar == 0) return -1; //this precedes other
            if (thisStar > 0 && otherStar == 0) return 1; //other precedes this
            if (thisStar == 0 && otherStar == 0)
            {
                return thisQmark - otherQmark;
            }

            return (other._filterExpression.Length - otherStar - otherQmark) - (this._filterExpression.Length - thisStar - thisQmark);
        }
    }
}
