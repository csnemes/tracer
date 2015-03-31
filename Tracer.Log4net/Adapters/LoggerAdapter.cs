using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using log4net.Util;

namespace Tracer.Log4net.Adapters
{
    public class LoggerAdapter
    {
        private readonly ILog _log;

        public LoggerAdapter(ILog log)
        {
            _log = log;
        }

        #region Methods required for trace enter and leave

        public void TraceEnter(string methodInfo)
        {
            _log.InfoFormat("Entered into {0}.", methodInfo);
        }

        public void TraceEnter(string methodInfo, string[] paramNames, object[] paramValues)
        {
            var parameters = new StringBuilder();
            for (int i = 0; i < paramNames.Length; i++)
            {
                parameters.AppendFormat("{0}={1} ,", paramNames[i], paramValues[i]);
            }
            parameters.Remove(parameters.Length - 2, 2); //remove unnecessary trailing space + comma
            _log.InfoFormat("Entered into {0} ({1}).", methodInfo, parameters);
        }

        public void TraceLeave(string methodInfo, long numberOfTicks)
        {
            _log.InfoFormat("Returned from {0}. Time taken: {1:0.00} ms.", methodInfo, ConvertTicksToMilliseconds(numberOfTicks));
        }

        public void TraceLeave(string methodInfo, long numberOfTicks, object returnValue)
        {
            _log.InfoFormat("Returned from {0} (returns={2}). Time taken: {1:0.00} ms.", methodInfo, ConvertTicksToMilliseconds(numberOfTicks), returnValue);
        }

        #endregion

        public void ErrorEvent(Exception exception) { }
        public void ErrorEvent(object message) { }
        public void ErrorEvent(object message, Exception exception) { }
        public void WarningEvent(Exception exception) { }
        public void WarningEvent(object message) { }
        public void WarningEvent(object message, Exception exception) { }
        public void WarningEvent(string format, params object[] paramInfo) { }

        public void LogInfoEvent(string methodInfo, object message)
        {
            _log.InfoFormat("{0}:{1}", methodInfo, message);    
        }

        public void InfoEvent(string format, params object[] paramInfo) { }
        public void DebugEvent(object message) { }
        public void DebugEvent(string format, params object[] paramInfo) { }
        public void TraceInfoEvent() { }
        public void TraceInfoEvent(object message) { }
        public void TraceInfoEvent(string format, params object[] paramInfo) { }
        public void VerboseEvent(object message) { }
        public void VerboseEvent(string format, params object[] paramInfo) { }

        private static double ConvertTicksToMilliseconds(long ticks)
        {
            //ticks * tickFrequency * 10000
            return ticks * (10000000 / (double)Stopwatch.Frequency) / 10000L;
        }
    }
}
