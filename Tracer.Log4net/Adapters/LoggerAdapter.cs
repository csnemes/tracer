using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tracer.Log4net.Adapters
{
    public class LoggerAdapter
    {
        #region Methods required to have trace enter and leave

        public void TraceEnter(string methodInfo)
        {
        }

        public void TraceEnter(string methodInfo, string[] paramNames, object[] paramValues)
        {
        }

        public void TraceLeave(string methodInfo, long numberOfTicks)
        {
        }

        public void TraceLeave(string methodInfo, long numberOfTicks, object returnValue)
        {
        }

        #endregion

        public void LogError(string methodInfo, Exception ex)
        { }

        public void LogError(string methodInfo, string message, Exception ex)
        { }
    }
}
