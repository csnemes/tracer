using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;

namespace Tracer.Fody.Weavers
{
    /// <summary>
    /// Interface is used to decide if a method needs tracing or not
    /// </summary>
    public interface ITraceLoggingFilter
    {
        bool ShouldAddTrace(MethodDefinition definition);
    }
}
