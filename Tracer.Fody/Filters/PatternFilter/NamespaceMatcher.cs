using System;
using System.Text.RegularExpressions;

namespace Tracer.Fody.Filters.PatternFilter
{
    public class NamespaceMatcher : IComparable<NamespaceMatcher>
    {
        private readonly Regex _regex;
        private readonly string _pattern;

        public NamespaceMatcher(string pattern)
        {
            _regex = CreateRegexFromPattern(pattern);
            _pattern = pattern;
        }

        public bool IsMatch(string input)
        {
            return _regex.IsMatch(input);
        }

        private Regex CreateRegexFromPattern(string pattern)
        {
            var regexPattern = pattern.Replace("?", "[a-z0-9_]").Replace("*", "[a-z0-9_]*").Replace("..", "[a-z0-9_.]*");

            regexPattern = "^" + regexPattern + "$";
            
            var result = new Regex(regexPattern, RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant);
            return result;
        }

        public int CompareTo(NamespaceMatcher other)
        {
            var otherElements = other._pattern.Split('.');
            var thisElements = this._pattern.Split('.');

            for (int idx = 0; idx < Math.Min(otherElements.Length, thisElements.Length); idx++)
            {
                if (otherElements[idx] == string.Empty && thisElements[idx] != string.Empty) return -1; //this precedes other
                if (otherElements[idx] != string.Empty && thisElements[idx] == string.Empty) return 1; //other precedes this
                if (otherElements[idx] == string.Empty && thisElements[idx] == string.Empty) continue;

                var otherHasStar = otherElements[idx].Contains("*");
                var thisHasStar = thisElements[idx].Contains("*");

                if (otherHasStar && !thisHasStar) return -1; //this precedes other
                if (thisHasStar && !otherHasStar) return 1; //other precedes this
            }

            return otherElements.Length - thisElements.Length;
        }
    }
}
