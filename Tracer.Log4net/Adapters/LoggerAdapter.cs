using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
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
        private const string NullString = "<NULL>";
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

        public void TraceEnter(string methodInfo, string[] paramNames, object[] paramValues)
        {
            if (_logger.IsEnabledFor(Level.Trace))
            {
                if (paramNames != null)
                {
                    var parameters = new StringBuilder();
                    for (int i = 0; i < paramNames.Length; i++)
                    {
                        parameters.AppendFormat("{0}={1}", paramNames[i], paramValues[i] ?? NullString);
                        if (i < paramNames.Length - 1) parameters.Append(", ");
                    }
                    Log(Level.Trace, methodInfo, String.Format("Entered into ({0}).", parameters));
                }
                else
                {
                    Log(Level.Trace, methodInfo, "Entered into.");
                }
            }
        }

        public void TraceLeave(string methodInfo, long numberOfTicks)
        {
            if (_logger.IsEnabledFor(Level.Trace))
            {
                Log(Level.Trace, methodInfo, String.Format("Returned from. Time taken: {0:0.00} ms.", ConvertTicksToMilliseconds(numberOfTicks)));
            }
        }

        public void TraceLeave(string methodInfo, long numberOfTicks, object returnValue)
        {
            if (_logger.IsEnabledFor(Level.Trace))
            {
                Log(Level.Trace, methodInfo,
                    String.Format("Returned from. (returns={1}). Time taken: {0:0.00} ms.",
                        ConvertTicksToMilliseconds(numberOfTicks), returnValue ?? NullString));
            }
        }

        #endregion

        #region ILog methods

        public void LogDebug(string methodInfo, object message)
        {
            if (_logger.IsEnabledFor(Level.Debug))
            {
                Log(Level.Debug, methodInfo, message);
            }
        }

        public void LogDebug(string methodInfo, object message, Exception exception)
        {
            if (_logger.IsEnabledFor(Level.Debug))
            {
                Log(Level.Debug, methodInfo, message, exception);
            }
        }

        public void LogDebugFormat(string methodInfo, string format, params object[] args)
        {
            if (_logger.IsEnabledFor(Level.Debug))
            {
                Log(Level.Debug, methodInfo, String.Format(format, args));
            }
        }

        public void LogDebugFormat(string methodInfo, string format, object arg0)
        {
            if (_logger.IsEnabledFor(Level.Debug))
            {
                Log(Level.Debug, methodInfo, String.Format(format, arg0));
            }
        }

        public void LogDebugFormat(string methodInfo, string format, object arg0, object arg1)
        {
            if (_logger.IsEnabledFor(Level.Debug))
            {
                Log(Level.Debug, methodInfo, String.Format(format, arg0, arg1));
            }
        }

        public void LogDebugFormat(string methodInfo, string format, object arg0, object arg1, object arg2)
        {
            if (_logger.IsEnabledFor(Level.Debug))
            {
                Log(Level.Debug, methodInfo, String.Format(format, arg0, arg1, arg2));
            }
        }

        public void LogDebugFormat(string methodInfo, IFormatProvider provider, string format, params object[] args)
        {
            if (_logger.IsEnabledFor(Level.Debug))
            {
                Log(Level.Debug, methodInfo, String.Format(provider, format, args));
            }
        }

        public void LogInfo(string methodInfo, object message)
        {
            if (_logger.IsEnabledFor(Level.Info))
            {
                Log(Level.Info, methodInfo, message);
            }
        }

        public void LogInfo(string methodInfo, object message, Exception exception)
        {
            if (_logger.IsEnabledFor(Level.Info))
            {
                Log(Level.Info, methodInfo, message, exception);
            }
        }

        public void LogInfoFormat(string methodInfo, string format, params object[] args)
        {
            if (_logger.IsEnabledFor(Level.Info))
            {
                Log(Level.Info, methodInfo, String.Format(format, args));
            }
        }

        public void LogInfoFormat(string methodInfo, string format, object arg0)
        {
            if (_logger.IsEnabledFor(Level.Info))
            {
                Log(Level.Info, methodInfo, String.Format(format, arg0));
            }
        }

        public void LogInfoFormat(string methodInfo, string format, object arg0, object arg1)
        {
            if (_logger.IsEnabledFor(Level.Info))
            {
                Log(Level.Info, methodInfo, String.Format(format, arg0, arg1));
            }
        }

        public void LogInfoFormat(string methodInfo, string format, object arg0, object arg1, object arg2)
        {
            if (_logger.IsEnabledFor(Level.Info))
            {
                Log(Level.Info, methodInfo, String.Format(format, arg0, arg1, arg2));
            }
        }

        public void LogInfoFormat(string methodInfo, IFormatProvider provider, string format, params object[] args)
        {
            if (_logger.IsEnabledFor(Level.Info))
            {
                Log(Level.Info, methodInfo, String.Format(provider, format, args));
            }
        }

        public void LogWarn(string methodInfo, object message)
        {
            if (_logger.IsEnabledFor(Level.Warn))
            {
                Log(Level.Warn, methodInfo, message);
            }
        }

        public void LogWarn(string methodInfo, object message, Exception exception)
        {
            if (_logger.IsEnabledFor(Level.Warn))
            {
                Log(Level.Warn, methodInfo, message, exception);
            }
        }

        public void LogWarnFormat(string methodInfo, string format, params object[] args)
        {
            if (_logger.IsEnabledFor(Level.Warn))
            {
                Log(Level.Warn, methodInfo, String.Format(format, args));
            }
        }

        public void LogWarnFormat(string methodInfo, string format, object arg0)
        {
            if (_logger.IsEnabledFor(Level.Warn))
            {
                Log(Level.Warn, methodInfo, String.Format(format, arg0));
            }
        }

        public void LogWarnFormat(string methodInfo, string format, object arg0, object arg1)
        {
            if (_logger.IsEnabledFor(Level.Warn))
            {
                Log(Level.Warn, methodInfo, String.Format(format, arg0, arg1));
            }
        }

        public void LogWarnFormat(string methodInfo, string format, object arg0, object arg1, object arg2)
        {
            if (_logger.IsEnabledFor(Level.Warn))
            {
                Log(Level.Warn, methodInfo, String.Format(format, arg0, arg1, arg2));
            }
        }

        public void LogWarnFormat(string methodInfo, IFormatProvider provider, string format, params object[] args)
        {
            if (_logger.IsEnabledFor(Level.Warn))
            {
                Log(Level.Warn, methodInfo, String.Format(provider, format, args));
            }
        }

        public void LogError(string methodInfo, object message)
        {
            if (_logger.IsEnabledFor(Level.Error))
            {
                Log(Level.Error, methodInfo, message);
            }
        }

        public void LogError(string methodInfo, object message, Exception exception)
        {
            if (_logger.IsEnabledFor(Level.Error))
            {
                Log(Level.Error, methodInfo, message, exception);
            }
        }

        public void LogErrorFormat(string methodInfo, string format, params object[] args)
        {
            if (_logger.IsEnabledFor(Level.Error))
            {
                Log(Level.Error, methodInfo, String.Format(format, args));
            }
        }

        public void LogErrorFormat(string methodInfo, string format, object arg0)
        {
            if (_logger.IsEnabledFor(Level.Error))
            {
                Log(Level.Error, methodInfo, String.Format(format, arg0));
            }
        }

        public void LogErrorFormat(string methodInfo, string format, object arg0, object arg1)
        {
            if (_logger.IsEnabledFor(Level.Error))
            {
                Log(Level.Error, methodInfo, String.Format(format, arg0, arg1));
            }
        }

        public void LogErrorFormat(string methodInfo, string format, object arg0, object arg1, object arg2)
        {
            if (_logger.IsEnabledFor(Level.Error))
            {
                Log(Level.Error, methodInfo, String.Format(format, arg0, arg1, arg2));
            }
        }

        public void LogErrorFormat(string methodInfo, IFormatProvider provider, string format, params object[] args)
        {
            if (_logger.IsEnabledFor(Level.Error))
            {
                Log(Level.Error, methodInfo, String.Format(provider, format, args));
            }
        }

        public void LogFatal(string methodInfo, object message)
        {
            if (_logger.IsEnabledFor(Level.Fatal))
            {
                Log(Level.Fatal, methodInfo, message);
            }
        }

        public void LogFatal(string methodInfo, object message, Exception exception)
        {
            if (_logger.IsEnabledFor(Level.Fatal))
            {
                Log(Level.Fatal, methodInfo, message, exception);
            }
        }

        public void LogFatalFormat(string methodInfo, string format, params object[] args)
        {
            if (_logger.IsEnabledFor(Level.Fatal))
            {
                Log(Level.Fatal, methodInfo, String.Format(format, args));
            }
        }

        public void LogFatalFormat(string methodInfo, string format, object arg0)
        {
            if (_logger.IsEnabledFor(Level.Fatal))
            {
                Log(Level.Fatal, methodInfo, String.Format(format, arg0));
            }
        }

        public void LogFatalFormat(string methodInfo, string format, object arg0, object arg1)
        {
            if (_logger.IsEnabledFor(Level.Fatal))
            {
                Log(Level.Fatal, methodInfo, String.Format(format, arg0, arg1));
            }
        }

        public void LogFatalFormat(string methodInfo, string format, object arg0, object arg1, object arg2)
        {
            if (_logger.IsEnabledFor(Level.Fatal))
            {
                Log(Level.Fatal, methodInfo, String.Format(format, arg0, arg1, arg2));
            }
        }

        public void LogFatalFormat(string methodInfo, IFormatProvider provider, string format, params object[] args)
        {
            if (_logger.IsEnabledFor(Level.Fatal))
            {
                Log(Level.Fatal, methodInfo, String.Format(provider, format, args));
            }
        }

        #endregion

        private void Log(Level level, string methodInfo, object message, Exception exception = null, PropertiesDictionary properties = null)
        {
            var eventData = new LoggingEventData()
            {
                LocationInfo = new LocationInfo(_typeName, methodInfo, "", ""),
                Level = level,
                Message = RenderedMessage(message),
                TimeStamp = DateTime.Now,
                LoggerName = _logger.Name,
                ThreadName = Thread.CurrentThread.Name,
                Domain = SystemInfo.ApplicationFriendlyName,
                ExceptionString = RenderException(exception),
                Properties = properties ?? new PropertiesDictionary()
            };

            _logger.Log(new LoggingEvent(eventData));
        }

        private string RenderedMessage(object message)
        {
            if (message == null)
            {
                return String.Empty;
            }
            else if (message is string)
            {
                return message as string;
            }
            else if (_logger.Repository != null)
            {
                return _logger.Repository.RendererMap.FindAndRender(message);
            }
            else
            {
                return message.ToString();
            }
        }

        private string RenderException(Exception exception)
        {
            if (exception == null) return String.Empty;

            if (_logger.Repository != null)
            {
                return _logger.Repository.RendererMap.FindAndRender(exception);
            }
            else
            {
                return exception.ToString();
            }
        }

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
