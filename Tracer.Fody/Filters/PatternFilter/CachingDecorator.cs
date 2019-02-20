using System.Collections.Generic;

namespace Tracer.Fody.Filters.PatternFilter
{
    public interface IMatcher<in TKey>
    {
        bool IsMatch(TKey input);
    }

    public class CachingDecorator<TKey> : IMatcher<TKey>
    {
        private readonly Dictionary<TKey, bool> _cache = new Dictionary<TKey, bool>();
        private readonly IMatcher<TKey> _delegate;

        public CachingDecorator(IMatcher<TKey> @delegate)
        {
            _delegate = @delegate;
        }

        public bool IsMatch(TKey input)
        {
            if (!_cache.TryGetValue(input, out var result))
            {
                result = _delegate.IsMatch(input);
                _cache.Add(input, result);
            }
            return result;
        }
    }
}
