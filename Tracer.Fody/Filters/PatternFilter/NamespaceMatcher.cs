using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Tracer.Fody.Filters.PatternFilter
{
    public class NamespaceMatcher
    {
        private readonly Regex _regex;
        private readonly int _order;

        public NamespaceMatcher(string pattern)
        {
            _regex = CreateRegexFromPattern(pattern);
            _order = pattern.Length - pattern.Count(it => it == '?') - 1000 * pattern.Count(it => it == '*');
        }

        public int Order => _order;

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
    }
}
