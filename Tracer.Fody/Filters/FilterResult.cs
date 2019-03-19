using System.Collections.Generic;

namespace Tracer.Fody.Filters
{
    public struct FilterResult
    {
        public bool ShouldTrace { get; }

        public Dictionary<string, string> Parameters { get; }

        public FilterResult(bool shouldTrace) : this()
        {
            ShouldTrace = shouldTrace;
        }

        public FilterResult(bool shouldTrace, Dictionary<string, string> parameters)
        {
            ShouldTrace = shouldTrace;
            Parameters = parameters;
        }
    }
}
