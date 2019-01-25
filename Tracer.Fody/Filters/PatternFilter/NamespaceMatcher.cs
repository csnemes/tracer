using System.Text.RegularExpressions;

namespace Tracer.Fody.Filters.PatternFilter
{
    public class NamespaceMatcher
    {
        private readonly Regex _regex;

        public NamespaceMatcher(string pattern)
        {
            _regex = CreateRegexFromPattern(pattern);
        }

        public bool IsMatch(string input)
        {
            return _regex.IsMatch(input);
        }

        private Regex CreateRegexFromPattern(string pattern)
        {
            var regexPattern = pattern.Replace("?", "[a-z0-9_]").Replace("*", "[a-z0-9_]*").Replace("..", "[a-z0-9_.]*");
            var result = new Regex(regexPattern, RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant);
            return result;
        }
    }
}
