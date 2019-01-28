using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Mono.Cecil;

namespace Tracer.Fody.Filters.PatternFilter
{
    public class ClassMatcher
    {
        private readonly Regex _regex;
        private readonly bool _matchPublic;
        private readonly bool _matchNonPublic;

        private static readonly string[] Keywords = { "public", "nonpublic", "internal" };

        private ClassMatcher(string regexPattern, bool matchPublic, bool matchNonPublic)
        {
            _regex = new Regex(regexPattern,
                RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant);

            _matchPublic = matchPublic;
            _matchNonPublic = matchNonPublic;

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
                className = className.Substring(0, backtickIndex);
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
                throw new ArgumentException("Filter expression cannot be empty.", nameof(filterExpression));

            var conditions = new List<string>();

            if (filterExpression.StartsWith("["))
            {
                var closingPosition = filterExpression.IndexOf(']');
                if (closingPosition == -1)
                    throw new ArgumentException("Missing closing square bracket in filter expression.",
                        nameof(filterExpression));

                var conditionsExpression = filterExpression.Substring(1, closingPosition - 1);
                filterExpression = filterExpression.Substring(closingPosition + 1);
                conditions = conditionsExpression.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(it => it.Trim().ToLowerInvariant())
                    .ToList();
            }

            if (filterExpression.Contains("]"))
                throw new ArgumentException("Invalid closing square bracket in filter expression.",
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
                conditions.Contains("nonpublic") || conditions.Contains("internal"));
        }
    }
}
