using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tracer.Fody.Helpers
{
    internal interface IWeavingLogger
    {
        void LogDebug(string message);

        void LogInfo(string message);

        void LogWarning(string message);

        void LogError(string message);
    }
}
