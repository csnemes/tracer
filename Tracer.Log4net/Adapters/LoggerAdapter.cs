using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using log4net.Core;
using log4net.Util;

namespace Tracer.Log4net.Adapters
{
    public class LoggerAdapter
    {
        private readonly ILogger _logger;
        private readonly Type _type;
        private readonly string _typeName;
        private readonly string _typeNamespace;

        public LoggerAdapter(Type type)
        {
            _type = type;
            _typeName = PrettyFormat(type);
            _typeNamespace = type.Namespace;
            _logger = LogManager.GetLogger(type).Logger;
        }

        #region Methods required for trace enter and leave

        public void TraceEnter(string methodInfo)
        {
            var eventData = new LoggingEventData()
            {
                LocationInfo = new LocationInfo(_typeName, methodInfo, "", ""),
                Level = Level.Trace,
                Message = "Entered into.",
                TimeStamp = DateTime.Now,
                LoggerName = _typeName,
                ThreadName = Thread.CurrentThread.Name,
                Domain = AppDomain.CurrentDomain.FriendlyName
            };
            _logger.Log(new LoggingEvent(eventData));
        }

        public void TraceEnter(string methodInfo, string[] paramNames, object[] paramValues)
        {
            var parameters = new StringBuilder();
            for (int i = 0; i < paramNames.Length; i++)
            {
                parameters.AppendFormat("{0}={1} ,", paramNames[i], paramValues[i]);
            }
            parameters.Remove(parameters.Length - 2, 2); //remove unnecessary trailing space + comma

            var eventData = new LoggingEventData()
            {
                LocationInfo = new LocationInfo(_typeName, methodInfo, "", ""),
                Level = Level.Trace,
                Message = String.Format("Entered into ({0}).", parameters),
                TimeStamp = DateTime.Now,
                LoggerName = _typeName,
                ThreadName = Thread.CurrentThread.Name,
                Domain = AppDomain.CurrentDomain.FriendlyName
            };
            _logger.Log(new LoggingEvent(eventData));
        }

        public void TraceLeave(string methodInfo, long numberOfTicks)
        {
            //_logger.InfoFormat("Returned from {0}. Time taken: {1:0.00} ms.", methodInfo, ConvertTicksToMilliseconds(numberOfTicks));
        }

        public void TraceLeave(string methodInfo, long numberOfTicks, object returnValue)
        {
            //_logger.InfoFormat("Returned from {0} (returns={2}). Time taken: {1:0.00} ms.", methodInfo, ConvertTicksToMilliseconds(numberOfTicks), returnValue);
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
            //_logger.InfoFormat("{0}:{1}", methodInfo, message);    
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

        private static string PrettyFormat(Type type)
        {
            var sb = new StringBuilder();
            if (type.IsGenericType)
            {
                sb.Append(type.Name.Remove(type.Name.IndexOf('`')));
                AddGenericPrettyFormat(sb, type.GenericTypeArguments);
            }
            else
            {
                sb.Append(type.Name);
            }
            return sb.ToString();
        }

        private static void AddGenericPrettyFormat(StringBuilder sb, Type[] genericArgumentTypes)
        {
            sb.Append("<");
            for (int i = 0; i < genericArgumentTypes.Length; i++)
            {
                sb.Append(genericArgumentTypes[i].Name);
                if (i < genericArgumentTypes.Length - 1) sb.Append(", ");
            }
            sb.Append(">");
        }
    }
}
