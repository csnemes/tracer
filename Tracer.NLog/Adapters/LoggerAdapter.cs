using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Text;
using NLog;

namespace Tracer.NLog.Adapters
{
    public class LoggerAdapter
    {
        private const string NullString = "<NULL>";
        private readonly ILogger _logger;
        private readonly Func<object, string, string> _renderParameterMethod;

        public LoggerAdapter(Type type)
        {
            _logger = LogManager.GetLogger(type.Name);
            var config = ConfigurationManager.AppSettings["LogUseSafeParameterRendering"];

            if ((config != null) && config.Equals("true", StringComparison.OrdinalIgnoreCase))
                _renderParameterMethod = GetSafeRenderedFormat;
            else
                _renderParameterMethod = GetRenderedFormat;
        }

        public bool LogIsDebugEnabled
        {
            get { return _logger.IsDebugEnabled; }
        }

        public bool LogIsErrorEnabled
        {
            get { return _logger.IsErrorEnabled; }
        }

        public bool LogIsFatalEnabled
        {
            get { return _logger.IsFatalEnabled; }
        }

        public bool LogIsInfoEnabled
        {
            get { return _logger.IsInfoEnabled; }
        }

        public bool LogIsWarnEnabled
        {
            get { return _logger.IsWarnEnabled; }
        }

        public void LogDebug(string methodInfo, object message)
        {
            if (_logger.IsDebugEnabled)
                Log(LogLevel.Debug, message.For(methodInfo));
        }

        public void LogDebug(string methodInfo, object message, Exception exception)
        {
            if (_logger.IsDebugEnabled)
                Log(LogLevel.Debug, message.For(methodInfo), exception);
        }

        public void LogDebugFormat(string methodInfo, string format, params object[] args)
        {
            if (_logger.IsDebugEnabled)
                Log(LogLevel.Debug, string.Format(format, args).For(methodInfo));
        }

        public void LogDebugFormat(string methodInfo, string format, object arg0)
        {
            if (_logger.IsDebugEnabled)
                Log(LogLevel.Debug, string.Format(format, arg0).For(methodInfo));
        }

        public void LogDebugFormat(string methodInfo, string format, object arg0, object arg1)
        {
            if (_logger.IsDebugEnabled)
                Log(LogLevel.Debug, string.Format(format, arg0, arg1).For(methodInfo));
        }

        public void LogDebugFormat(string methodInfo, string format, object arg0, object arg1, object arg2)
        {
            if (_logger.IsDebugEnabled)
                Log(LogLevel.Debug, string.Format(format, arg0, arg1, arg2).For(methodInfo));
        }

        public void LogDebugFormat(string methodInfo, IFormatProvider provider, string format, params object[] args)
        {
            if (_logger.IsDebugEnabled)
                Log(LogLevel.Debug, string.Format(provider, format, args).For(methodInfo));
        }

        public void LogError(string methodInfo, object message)
        {
            if (_logger.IsErrorEnabled)
                Log(LogLevel.Error, message.For(methodInfo));
        }

        public void LogError(string methodInfo, object message, Exception exception)
        {
            if (_logger.IsErrorEnabled)
                Log(LogLevel.Error, message.For(methodInfo), exception);
        }

        public void LogErrorFormat(string methodInfo, string format, params object[] args)
        {
            if (_logger.IsErrorEnabled)
                Log(LogLevel.Error, string.Format(format, args).For(methodInfo));
        }

        public void LogErrorFormat(string methodInfo, string format, object arg0)
        {
            if (_logger.IsErrorEnabled)
                Log(LogLevel.Error, string.Format(format, arg0).For(methodInfo));
        }

        public void LogErrorFormat(string methodInfo, string format, object arg0, object arg1)
        {
            if (_logger.IsErrorEnabled)
                Log(LogLevel.Error, string.Format(format, arg0, arg1).For(methodInfo));
        }

        public void LogErrorFormat(string methodInfo, string format, object arg0, object arg1, object arg2)
        {
            if (_logger.IsErrorEnabled)
                Log(LogLevel.Error, string.Format(format, arg0, arg1, arg2).For(methodInfo));
        }

        public void LogErrorFormat(string methodInfo, IFormatProvider provider, string format, params object[] args)
        {
            if (_logger.IsErrorEnabled)
                Log(LogLevel.Error, string.Format(provider, format, args).For(methodInfo));
        }

        public void LogFatal(string methodInfo, object message)
        {
            if (_logger.IsFatalEnabled)
                Log(LogLevel.Fatal, message.For(methodInfo));
        }

        public void LogFatal(string methodInfo, object message, Exception exception)
        {
            if (_logger.IsFatalEnabled)
                Log(LogLevel.Fatal, message.For(methodInfo), exception);
        }

        public void LogFatalFormat(string methodInfo, string format, params object[] args)
        {
            if (_logger.IsFatalEnabled)
                Log(LogLevel.Fatal, string.Format(format, args).For(methodInfo));
        }

        public void LogFatalFormat(string methodInfo, string format, object arg0)
        {
            if (_logger.IsFatalEnabled)
                Log(LogLevel.Fatal, string.Format(format, arg0).For(methodInfo));
        }

        public void LogFatalFormat(string methodInfo, string format, object arg0, object arg1)
        {
            if (_logger.IsFatalEnabled)
                Log(LogLevel.Fatal, string.Format(format, arg0, arg1).For(methodInfo));
        }

        public void LogFatalFormat(string methodInfo, string format, object arg0, object arg1, object arg2)
        {
            if (_logger.IsFatalEnabled)
                Log(LogLevel.Fatal, string.Format(format, arg0, arg1, arg2).For(methodInfo));
        }

        public void LogFatalFormat(string methodInfo, IFormatProvider provider, string format, params object[] args)
        {
            if (_logger.IsFatalEnabled)
                Log(LogLevel.Fatal, string.Format(provider, format, args).For(methodInfo));
        }

        public void LogInfo(string methodInfo, object message)
        {
            if (_logger.IsInfoEnabled)
                Log(LogLevel.Info, message.For(methodInfo));
        }

        public void LogInfo(string methodInfo, object message, Exception exception)
        {
            if (_logger.IsInfoEnabled)
                Log(LogLevel.Info, message.For(methodInfo), exception);
        }

        public void LogInfoFormat(string methodInfo, string format, params object[] args)
        {
            if (_logger.IsInfoEnabled)
                Log(LogLevel.Info, string.Format(format, args).For(methodInfo));
        }

        public void LogInfoFormat(string methodInfo, string format, object arg0)
        {
            if (_logger.IsInfoEnabled)
                Log(LogLevel.Info, string.Format(format, arg0).For(methodInfo));
        }

        public void LogInfoFormat(string methodInfo, string format, object arg0, object arg1)
        {
            if (_logger.IsInfoEnabled)
                Log(LogLevel.Info, string.Format(format, arg0, arg1).For(methodInfo));
        }

        public void LogInfoFormat(string methodInfo, string format, object arg0, object arg1, object arg2)
        {
            if (_logger.IsInfoEnabled)
                Log(LogLevel.Info, string.Format(format, arg0, arg1, arg2).For(methodInfo));
        }

        public void LogInfoFormat(string methodInfo, IFormatProvider provider, string format, params object[] args)
        {
            if (_logger.IsInfoEnabled)
                Log(LogLevel.Info, string.Format(provider, format, args).For(methodInfo));
        }

        public void LogWarn(string methodInfo, object message)
        {
            if (_logger.IsWarnEnabled)
                Log(LogLevel.Warn, message.For(methodInfo));
        }

        public void LogWarn(string methodInfo, object message, Exception exception)
        {
            if (_logger.IsWarnEnabled)
                Log(LogLevel.Warn, message.For(methodInfo), exception);
        }

        public void LogWarnFormat(string methodInfo, string format, params object[] args)
        {
            if (_logger.IsWarnEnabled)
                Log(LogLevel.Warn, string.Format(format, args).For(methodInfo));
        }

        public void LogWarnFormat(string methodInfo, string format, object arg0)
        {
            if (_logger.IsWarnEnabled)
                Log(LogLevel.Warn, string.Format(format, arg0).For(methodInfo));
        }

        public void LogWarnFormat(string methodInfo, string format, object arg0, object arg1)
        {
            if (_logger.IsWarnEnabled)
                Log(LogLevel.Warn, string.Format(format, arg0, arg1).For(methodInfo));
        }

        public void LogWarnFormat(string methodInfo, string format, object arg0, object arg1, object arg2)
        {
            if (_logger.IsWarnEnabled)
                Log(LogLevel.Warn, string.Format(format, arg0, arg1, arg2).For(methodInfo));
        }

        public void LogWarnFormat(string methodInfo, IFormatProvider provider, string format, params object[] args)
        {
            if (_logger.IsWarnEnabled)
                Log(LogLevel.Warn, string.Format(provider, format, args).For(methodInfo));
        }

        public void TraceEnter(string methodInfo, string[] paramNames, object[] paramValues)
        {
            if (_logger.IsTraceEnabled)
            {
                string message;
                var propDict = new Dictionary<string, object>();
                propDict["trace"] = "ENTER";

                if (paramNames != null)
                {
                    var parameters = new StringBuilder();
                    for (var i = 0; i < paramNames.Length; i++)
                    {
                        parameters.AppendFormat("{0}={1}", paramNames[i], _renderParameterMethod(paramValues[i], NullString));
                        if (i < paramNames.Length - 1) parameters.Append(", ");
                    }
                    var argInfo = parameters.ToString();
                    propDict["arguments"] = argInfo;
                    message = string.Format("Entered into {0} ({1}).", methodInfo, argInfo);
                }
                else
                {
                    message = string.Format("Entered into {0}.", methodInfo);
                }
                Log(LogLevel.Trace, message, null, propDict);
            }
        }

        public void TraceLeave(string methodInfo, long startTicks, long endTicks, string[] paramNames, object[] paramValues)
        {
            if (_logger.IsTraceEnabled)
            {
                var propDict = new Dictionary<string, object>();
                propDict["trace"] = "LEAVE";

                string returnValue = null;
                if (paramNames != null)
                {
                    var parameters = new StringBuilder();
                    for (var i = 0; i < paramNames.Length; i++)
                    {
                        parameters.AppendFormat("{0}={1}", paramNames[i] ?? "$return", _renderParameterMethod(paramValues[i], NullString));
                        if (i < paramNames.Length - 1) parameters.Append(", ");
                    }
                    returnValue = parameters.ToString();
                    propDict["arguments"] = returnValue;
                }

                var timeTaken = ConvertTicksToMilliseconds(endTicks - startTicks);
                propDict["startTicks"] = startTicks;
                propDict["endTicks"] = endTicks;
                propDict["timeTaken"] = timeTaken;

                Log(LogLevel.Trace, string.Format("Returned from {1} ({2}). Time taken: {0:0.00} ms.", timeTaken, methodInfo, returnValue), null, propDict);
            }
        }

        private static void AddGenericPrettyFormat(StringBuilder sb, Type[] genericArgumentTypes)
        {
            sb.Append("<");
            for (var i = 0; i < genericArgumentTypes.Length; i++)
            {
                sb.Append(genericArgumentTypes[i].Name);
                if (i < genericArgumentTypes.Length - 1) sb.Append(", ");
            }
            sb.Append(">");
        }

        private static double ConvertTicksToMilliseconds(long ticks)
        {
            //ticks * tickFrequency * 10000
            return ticks*(10000000/(double) Stopwatch.Frequency)/10000L;
        }

        private static string PrettyFormat(Type type)
        {
            var sb = new StringBuilder();
            if (type.IsGenericType)
            {
                sb.Append(type.Name.Remove(type.Name.IndexOf('`')));
                AddGenericPrettyFormat(sb, type.GetGenericArguments());
            }
            else
            {
                sb.Append(type.Name);
            }
            return sb.ToString();
        }

        private string GetRenderedFormat(object message, string stringRepresentationOfNull = "")
        {
            if (message == null)
                return stringRepresentationOfNull;
            if (message is string)
                return (string) message;
            return message.ToString();
        }

        private string GetSafeRenderedFormat(object message, string stringRepresentationOfNull = "")
        {
            if (message == null)
                return stringRepresentationOfNull;

            var str = message as string;
            if (str != null)
                return str;

            return message.ToString();
        }

        private void Log(LogLevel level, object message, Exception exception = null, Dictionary<string, object> properties = null)
        {
            var eventData = new LogEventInfo();
            eventData.Exception = exception;
            eventData.Message = GetRenderedFormat(message);
            eventData.Level = level;

            if (properties != null)
                foreach (var property in properties)
                    eventData.Properties.Add(property.Key, property.Value);
            _logger.Log(eventData);
        }
    }

    internal static class StringExtensions
    {
        public static string For(this object msg, string subject)
        {
            return $"{subject} --> {msg}";
        }
    }
}