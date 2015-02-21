using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;

namespace Tracer.Fody.Weavers
{
    internal interface ILoggerScopeProvider
    {
        IMetadataScope GetLoggerScope();
    }
}
