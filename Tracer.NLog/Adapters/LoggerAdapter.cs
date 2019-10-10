using System;
using System.Diagnostics;
using System.Text;
using NLog;

namespace Tracer.NLog.Adapters
{
    public class LoggerAdapter
    {
        private const string NullString = "<NULL>";
        private readonly ILogger _logger;
        private readonly string _typeName;
        private readonly string _specialPrefix;

        public LoggerAdapter(Type type)
        {
            _typeName = PrettyFormat(type);
            _logger = LogManager.GetLogger(type.ToString());
            var configPrefix = Environment.GetEnvironmentVariable("TracerFodySpecialKeyPrefix");
            _specialPrefix = string.IsNullOrWhiteSpace(configPrefix) ? "$" : configPrefix;
        }

        #region Methods required for trace enter and leave

        public void TraceEnter(string methodInfo, Tuple<string, string>[] methodParameters, string[] paramNames, object[] paramValues)
        {
            if (_logger.IsTraceEnabled)
            {
                string argInfo = CaptureArguementsInfo(paramNames, paramValues);

                var logEvent = string.IsNullOrEmpty(argInfo) ?
                    LogEventInfo.Create(LogLevel.Trace, _logger.Name, null, "Entered into {0}.", new object[] { methodInfo }) :
                    LogEventInfo.Create(LogLevel.Trace, _logger.Name, null, "Entered into {0} ({1}).", new object[] { methodInfo, argInfo });
                logEvent.Properties["trace"] = "ENTER";
                logEvent.Properties["arguments"] = argInfo;
                LogEvent(methodInfo, logEvent);
            }
        }

        public void TraceLeave(string methodInfo, Tuple<string, string>[] methodParameters, long startTicks, long endTicks, string[] paramNames, object[] paramValues)
        {
            if (_logger.IsTraceEnabled)
            {
                var timeTaken = ConvertTicksToMilliseconds(endTicks - startTicks);

                string argInfo = CaptureArguementsInfo(paramNames, paramValues);

                var logEvent = LogEventInfo.Create(LogLevel.Trace, _logger.Name, null, "Returned from {1} ({2}). Time taken: {0:0.00} ms.", new object[] { timeTaken, methodInfo, argInfo });
                logEvent.Properties["trace"] = "LEAVE";
                logEvent.Properties["arguments"] = argInfo;
                logEvent.Properties["startTicks"] = startTicks;
                logEvent.Properties["endTicks"] = endTicks;
                logEvent.Properties["timeTaken"] = timeTaken;
                LogEvent(methodInfo, logEvent);
            }
        }

        private string CaptureArguementsInfo(string[] paramNames, object[] paramValues)
        {
            string argInfo = string.Empty;
            if (paramNames?.Length > 0)
            {
                var parameters = new StringBuilder();
                for (int i = 0; i < paramNames.Length; i++)
                {
                    if (parameters.Length > 0)
                        parameters.Append(", ");
                    parameters.Append(FixSpecialParameterName(paramNames[i] ?? "$return"));
                    parameters.Append('=');
                    parameters.Append(GetRenderedFormat(paramValues[i], NullString));
                }
                argInfo = parameters.ToString();
            }

            return argInfo;
        }

        private string FixSpecialParameterName(string paramName)
        {
            if (paramName[0] == '$')
            {
                return _specialPrefix + paramName.Substring(1);
            }

            return paramName;
        }

        private static double ConvertTicksToMilliseconds(long ticks)
        {
            //ticks * tickFrequency * 10000
            return ticks * (10000000 / (double)Stopwatch.Frequency) / 10000L;
        }

        private string GetRenderedFormat(object message, string stringRepresentationOfNull = "")
        {
            if (message == null)
                return stringRepresentationOfNull;
            return message.ToString();
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

        private static string PrettyFormat(Type type)
        {
            var sb = new StringBuilder();
            sb.Append(type.Namespace);
            sb.Append(".");
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

        private void LogValue<T>(LogLevel logLevel, string methodInfo, T value, IFormatProvider formatProvider = null)
        {
            var logEvent = LogEventInfo.Create(logLevel, _logger.Name, formatProvider, value);
            LogEvent(methodInfo, logEvent);
        }

        private void LogMessage(LogLevel logLevel, string methodInfo, Exception exception, string message, IFormatProvider formatProvider = null, object[] args = null)
        {
            var logEvent = LogEventInfo.Create(logLevel, _logger.Name, exception, formatProvider, message, args);
            LogEvent(methodInfo, logEvent);
        }

        private void LogEvent(string methodInfo, LogEventInfo logEvent)
        {
            if (!string.IsNullOrEmpty(methodInfo))
                logEvent.SetCallerInfo(_typeName, methodInfo, string.Empty, 0);
            _logger.Log(typeof(LoggerAdapter), logEvent);
        }

        #endregion

        public ILogger LogOriginalLogger
        {
            get { return _logger; }
        }

        public bool LogIsTraceEnabled
        {
            get { return _logger.IsTraceEnabled; }
        }

        public bool LogIsDebugEnabled
        {
            get { return _logger.IsDebugEnabled; }
        }

        public bool LogIsInfoEnabled
        {
            get { return _logger.IsInfoEnabled; }
        }

        public bool LogIsWarnEnabled
        {
            get { return _logger.IsWarnEnabled; }
        }

        public bool LogIsErrorEnabled
        {
            get { return _logger.IsErrorEnabled; }
        }

        public bool LogIsFatalEnabled
        {
            get { return _logger.IsFatalEnabled; }
        }

        #region Trace() overloads 

        public void LogTrace<T>(string methodInfo, T value)
        {
            if (_logger.IsTraceEnabled)
                LogValue(LogLevel.Trace, methodInfo, value);
        }

        public void LogTrace<T>(string methodInfo, IFormatProvider formatProvider, T value)
        {
            if (_logger.IsTraceEnabled)
                LogValue(LogLevel.Trace, methodInfo, value, formatProvider);
        }

        public void LogTrace(string methodInfo, LogMessageGenerator messageFunc)
        {
            if (_logger.IsTraceEnabled)
                LogMessage(LogLevel.Trace, methodInfo, null, messageFunc());
        }

        public void LogTraceException(string methodInfo, string message, Exception exception)
        {
            if (_logger.IsTraceEnabled)
                LogMessage(LogLevel.Trace, methodInfo, exception, message);
        }

        public void LogTrace(string methodInfo, Exception exception, string message)
        {
            if (_logger.IsTraceEnabled)
                LogMessage(LogLevel.Trace, methodInfo, exception, message);
        }

        public void LogTrace(string methodInfo, Exception exception, string message, params object[] args)
        {
            if (_logger.IsTraceEnabled)
                LogMessage(LogLevel.Trace, methodInfo, exception, message, null, args);
        }

        public void LogTrace(string methodInfo, Exception exception, IFormatProvider formatProvider, string message,
            params object[] args)
        {
            if (_logger.IsTraceEnabled)
                LogMessage(LogLevel.Trace, methodInfo, exception, message, formatProvider, args);
        }

        public void LogTrace(string methodInfo, IFormatProvider formatProvider, string message, params object[] args)
        {
            if (_logger.IsTraceEnabled)
                LogMessage(LogLevel.Trace, methodInfo, null, message, formatProvider, args);
        }

        public void LogTrace(string methodInfo, string message)
        {
            if (_logger.IsTraceEnabled)
                LogMessage(LogLevel.Trace, methodInfo, null, message);
        }

        public void LogTrace(string methodInfo, string message, params object[] args)
        {
            if (_logger.IsTraceEnabled)
                LogMessage(LogLevel.Trace, methodInfo, null, message, null, args);
        }

        public void LogTrace(string methodInfo, string message, Exception exception)
        {
            if (_logger.IsTraceEnabled)
                LogMessage(LogLevel.Trace, methodInfo, exception, message);
        }

        public void LogTrace<TArgument>(string methodInfo, IFormatProvider formatProvider, string message,
            TArgument argument)
        {
            if (_logger.IsTraceEnabled)
                LogMessage(LogLevel.Trace, methodInfo, null, message, formatProvider, new object[] { argument });
        }

        public void LogTrace<TArgument>(string methodInfo, string message, TArgument argument)
        {
            if (_logger.IsTraceEnabled)
                LogMessage(LogLevel.Trace, methodInfo, null, message, null, new object[] { argument });
        }

        public void LogTrace<TArgument1, TArgument2>(string methodInfo, IFormatProvider formatProvider, string message,
            TArgument1 argument1, TArgument2 argument2)
        {
            if (_logger.IsTraceEnabled)
                LogMessage(LogLevel.Trace, methodInfo, null, message, formatProvider, new object[] { argument1, argument2 });
        }

        public void LogTrace<TArgument1, TArgument2>(string methodInfo, string message, TArgument1 argument1,
            TArgument2 argument2)
        {
            if (_logger.IsTraceEnabled)
                LogMessage(LogLevel.Trace, methodInfo, null, message, null, new object[] { argument1, argument2 });
        }

        public void LogTrace<TArgument1, TArgument2, TArgument3>(string methodInfo, IFormatProvider formatProvider,
            string message, TArgument1 argument1, TArgument2 argument2, TArgument3 argument3)
        {
            if (_logger.IsTraceEnabled)
                LogMessage(LogLevel.Trace, methodInfo, null, message, formatProvider, new object[] { argument1, argument2, argument3 });
        }

        public void LogTrace<TArgument1, TArgument2, TArgument3>(string methodInfo, string message, TArgument1 argument1,
            TArgument2 argument2, TArgument3 argument3)
        {
            if (_logger.IsTraceEnabled)
                LogMessage(LogLevel.Trace, methodInfo, null, message, null, new object[] { argument1, argument2, argument3 });
        }

        #endregion

        #region Debug() overloads 

        public void LogDebug<T>(string methodInfo, T value)
        {
            if (_logger.IsDebugEnabled)
                LogValue(LogLevel.Debug, methodInfo, value);
        }

        public void LogDebug<T>(string methodInfo, IFormatProvider formatProvider, T value)
        {
            if (_logger.IsDebugEnabled)
                LogValue(LogLevel.Debug, methodInfo, value, formatProvider);
        }

        public void LogDebug(string methodInfo, LogMessageGenerator messageFunc)
        {
            if (_logger.IsDebugEnabled)
                LogMessage(LogLevel.Debug, methodInfo, null, messageFunc());
        }

        public void LogDebugException(string methodInfo, string message, Exception exception)
        {
            if (_logger.IsDebugEnabled)
                LogMessage(LogLevel.Debug, methodInfo, exception, message);
        }

        public void LogDebug(string methodInfo, Exception exception, string message)
        {
            if (_logger.IsDebugEnabled)
                LogMessage(LogLevel.Debug, methodInfo, exception, message);
        }

        public void LogDebug(string methodInfo, Exception exception, string message, params object[] args)
        {
            if (_logger.IsDebugEnabled)
                LogMessage(LogLevel.Debug, methodInfo, exception, message, null, args);
        }

        public void LogDebug(string methodInfo, Exception exception, IFormatProvider formatProvider, string message,
            params object[] args)
        {
            if (_logger.IsDebugEnabled)
                LogMessage(LogLevel.Debug, methodInfo, exception, message, formatProvider, args);
        }

        public void LogDebug(string methodInfo, IFormatProvider formatProvider, string message, params object[] args)
        {
            if (_logger.IsDebugEnabled)
                LogMessage(LogLevel.Debug, methodInfo, null, message, formatProvider, args);
        }

        public void LogDebug(string methodInfo, string message)
        {
            if (_logger.IsDebugEnabled)
                LogMessage(LogLevel.Debug, methodInfo, null, message);
        }

        public void LogDebug(string methodInfo, string message, params object[] args)
        {
            if (_logger.IsDebugEnabled)
                LogMessage(LogLevel.Debug, methodInfo, null, message, null, args);
        }

        public void LogDebug(string methodInfo, string message, Exception exception)
        {
            if (_logger.IsDebugEnabled)
                LogMessage(LogLevel.Debug, methodInfo, exception, message);
        }

        public void LogDebug<TArgument>(string methodInfo, IFormatProvider formatProvider, string message,
            TArgument argument)
        {
            if (_logger.IsDebugEnabled)
                LogMessage(LogLevel.Debug, methodInfo, null, message, formatProvider, new object[] { argument });
        }

        public void LogDebug<TArgument>(string methodInfo, string message, TArgument argument)
        {
            if (_logger.IsDebugEnabled)
                LogMessage(LogLevel.Debug, methodInfo, null, message, null, new object[] { argument });
        }

        public void LogDebug<TArgument1, TArgument2>(string methodInfo, IFormatProvider formatProvider, string message,
            TArgument1 argument1, TArgument2 argument2)
        {
            if (_logger.IsDebugEnabled)
                LogMessage(LogLevel.Debug, methodInfo, null, message, formatProvider, new object[] { argument1, argument2 });
        }

        public void LogDebug<TArgument1, TArgument2>(string methodInfo, string message, TArgument1 argument1,
            TArgument2 argument2)
        {
            if (_logger.IsDebugEnabled)
                LogMessage(LogLevel.Debug, methodInfo, null, message, null, new object[] { argument1, argument2 });
        }

        public void LogDebug<TArgument1, TArgument2, TArgument3>(string methodInfo, IFormatProvider formatProvider,
            string message, TArgument1 argument1, TArgument2 argument2, TArgument3 argument3)
        {
            if (_logger.IsDebugEnabled)
                LogMessage(LogLevel.Debug, methodInfo, null, message, formatProvider, new object[] { argument1, argument2, argument3 });
        }

        public void LogDebug<TArgument1, TArgument2, TArgument3>(string methodInfo, string message, TArgument1 argument1,
            TArgument2 argument2, TArgument3 argument3)
        {
            if (_logger.IsDebugEnabled)
                LogMessage(LogLevel.Debug, methodInfo, null, message, null, new object[] { argument1, argument2, argument3 });
        }

        #endregion

        #region Info() overloads 

        public void LogInfo<T>(string methodInfo, T value)
        {
            if (_logger.IsInfoEnabled)
                LogValue(LogLevel.Info, methodInfo, value);
        }

        public void LogInfo<T>(string methodInfo, IFormatProvider formatProvider, T value)
        {
            if (_logger.IsInfoEnabled)
                LogValue(LogLevel.Info, methodInfo, value, formatProvider);
        }

        public void LogInfo(string methodInfo, LogMessageGenerator messageFunc)
        {
            if (_logger.IsInfoEnabled)
                LogMessage(LogLevel.Info, methodInfo, null, messageFunc());
        }

        public void LogInfoException(string methodInfo, string message, Exception exception)
        {
            if (_logger.IsInfoEnabled)
                LogMessage(LogLevel.Info, methodInfo, exception, message);
        }

        public void LogInfo(string methodInfo, Exception exception, string message)
        {
            if (_logger.IsInfoEnabled)
                LogMessage(LogLevel.Info, methodInfo, exception, message);
        }

        public void LogInfo(string methodInfo, Exception exception, string message, params object[] args)
        {
            if (_logger.IsInfoEnabled)
                LogMessage(LogLevel.Info, methodInfo, exception, message, null, args);
        }

        public void LogInfo(string methodInfo, Exception exception, IFormatProvider formatProvider, string message,
            params object[] args)
        {
            if (_logger.IsInfoEnabled)
                LogMessage(LogLevel.Info, methodInfo, exception, message, formatProvider, args);
        }

        public void LogInfo(string methodInfo, IFormatProvider formatProvider, string message, params object[] args)
        {
            if (_logger.IsInfoEnabled)
                LogMessage(LogLevel.Info, methodInfo, null, message, formatProvider, args);
        }

        public void LogInfo(string methodInfo, string message)
        {
            if (_logger.IsInfoEnabled)
                LogMessage(LogLevel.Info, methodInfo, null, message);
        }

        public void LogInfo(string methodInfo, string message, params object[] args)
        {
            if (_logger.IsInfoEnabled)
                LogMessage(LogLevel.Info, methodInfo, null, message, null, args);
        }

        public void LogInfo(string methodInfo, string message, Exception exception)
        {
            if (_logger.IsInfoEnabled)
                LogMessage(LogLevel.Info, methodInfo, exception, message);
        }

        public void LogInfo<TArgument>(string methodInfo, IFormatProvider formatProvider, string message,
            TArgument argument)
        {
            if (_logger.IsInfoEnabled)
                LogMessage(LogLevel.Info, methodInfo, null, message, formatProvider, new object[] { argument });
        }

        public void LogInfo<TArgument>(string methodInfo, string message, TArgument argument)
        {
            if (_logger.IsInfoEnabled)
                LogMessage(LogLevel.Info, methodInfo, null, message, null, new object[] { argument });
        }

        public void LogInfo<TArgument1, TArgument2>(string methodInfo, IFormatProvider formatProvider, string message,
            TArgument1 argument1, TArgument2 argument2)
        {
            if (_logger.IsInfoEnabled)
                LogMessage(LogLevel.Info, methodInfo, null, message, formatProvider, new object[] { argument1, argument2 });
        }

        public void LogInfo<TArgument1, TArgument2>(string methodInfo, string message, TArgument1 argument1,
            TArgument2 argument2)
        {
            if (_logger.IsInfoEnabled)
                LogMessage(LogLevel.Info, methodInfo, null, message, null, new object[] { argument1, argument2 });
        }

        public void LogInfo<TArgument1, TArgument2, TArgument3>(string methodInfo, IFormatProvider formatProvider,
            string message, TArgument1 argument1, TArgument2 argument2, TArgument3 argument3)
        {
            if (_logger.IsInfoEnabled)
                LogMessage(LogLevel.Info, methodInfo, null, message, formatProvider, new object[] { argument1, argument2, argument3 });
        }

        public void LogInfo<TArgument1, TArgument2, TArgument3>(string methodInfo, string message, TArgument1 argument1,
            TArgument2 argument2, TArgument3 argument3)
        {
            if (_logger.IsInfoEnabled)
                LogMessage(LogLevel.Info, methodInfo, null, message, null, new object[] { argument1, argument2, argument3 });
        }


        #endregion

        #region Warn() overloads 

        public void LogWarn<T>(string methodInfo, T value)
        {
            if (_logger.IsWarnEnabled)
                LogValue(LogLevel.Warn, methodInfo, value);
        }

        public void LogWarn<T>(string methodInfo, IFormatProvider formatProvider, T value)
        {
            if (_logger.IsWarnEnabled)
                LogValue(LogLevel.Warn, methodInfo, value, formatProvider);
        }

        public void LogWarn(string methodInfo, LogMessageGenerator messageFunc)
        {
            if (_logger.IsWarnEnabled)
                LogMessage(LogLevel.Warn, methodInfo, null, messageFunc());
        }

        public void LogWarnException(string methodInfo, string message, Exception exception)
        {
            if (_logger.IsWarnEnabled)
                LogMessage(LogLevel.Warn, methodInfo, exception, message);
        }

        public void LogWarn(string methodInfo, Exception exception, string message)
        {
            if (_logger.IsWarnEnabled)
                LogMessage(LogLevel.Warn, methodInfo, exception, message);
        }

        public void LogWarn(string methodInfo, Exception exception, string message, params object[] args)
        {
            if (_logger.IsWarnEnabled)
                LogMessage(LogLevel.Warn, methodInfo, exception, message, null, args);
        }

        public void LogWarn(string methodInfo, Exception exception, IFormatProvider formatProvider, string message,
            params object[] args)
        {
            if (_logger.IsWarnEnabled)
                LogMessage(LogLevel.Warn, methodInfo, exception, message, formatProvider, args);
        }

        public void LogWarn(string methodInfo, IFormatProvider formatProvider, string message, params object[] args)
        {
            if (_logger.IsWarnEnabled)
                LogMessage(LogLevel.Warn, methodInfo, null, message, formatProvider, args);
        }

        public void LogWarn(string methodInfo, string message)
        {
            if (_logger.IsWarnEnabled)
                LogMessage(LogLevel.Warn, methodInfo, null, message);
        }

        public void LogWarn(string methodInfo, string message, params object[] args)
        {
            if (_logger.IsWarnEnabled)
                LogMessage(LogLevel.Warn, methodInfo, null, message, null, args);
        }

        public void LogWarn(string methodInfo, string message, Exception exception)
        {
            if (_logger.IsWarnEnabled)
                LogMessage(LogLevel.Warn, methodInfo, exception, message);
        }

        public void LogWarn<TArgument>(string methodInfo, IFormatProvider formatProvider, string message,
            TArgument argument)
        {
            if (_logger.IsWarnEnabled)
                LogMessage(LogLevel.Warn, methodInfo, null, message, formatProvider, new object[] { argument });
        }

        public void LogWarn<TArgument>(string methodInfo, string message, TArgument argument)
        {
            if (_logger.IsWarnEnabled)
                LogMessage(LogLevel.Warn, methodInfo, null, message, null, new object[] { argument });
        }

        public void LogWarn<TArgument1, TArgument2>(string methodInfo, IFormatProvider formatProvider, string message,
            TArgument1 argument1, TArgument2 argument2)
        {
            if (_logger.IsWarnEnabled)
                LogMessage(LogLevel.Warn, methodInfo, null, message, formatProvider, new object[] { argument1, argument2 });
        }

        public void LogWarn<TArgument1, TArgument2>(string methodInfo, string message, TArgument1 argument1,
            TArgument2 argument2)
        {
            if (_logger.IsWarnEnabled)
                LogMessage(LogLevel.Warn, methodInfo, null, message, null, new object[] { argument1, argument2 });
        }

        public void LogWarn<TArgument1, TArgument2, TArgument3>(string methodInfo, IFormatProvider formatProvider,
            string message, TArgument1 argument1, TArgument2 argument2, TArgument3 argument3)
        {
            if (_logger.IsWarnEnabled)
                LogMessage(LogLevel.Warn, methodInfo, null, message, formatProvider, new object[] { argument1, argument2, argument3 });
        }

        public void LogWarn<TArgument1, TArgument2, TArgument3>(string methodInfo, string message, TArgument1 argument1,
            TArgument2 argument2, TArgument3 argument3)
        {
            if (_logger.IsWarnEnabled)
                LogMessage(LogLevel.Warn, methodInfo, null, message, null, new object[] { argument1, argument2, argument3 });
        }


        #endregion

        #region Error() overloads 

        public void LogError<T>(string methodInfo, T value)
        {
            if (_logger.IsErrorEnabled)
                LogValue(LogLevel.Error, methodInfo, value);
        }

        public void LogError<T>(string methodInfo, IFormatProvider formatProvider, T value)
        {
            if (_logger.IsErrorEnabled)
                LogValue(LogLevel.Error, methodInfo, value, formatProvider);
        }

        public void LogError(string methodInfo, LogMessageGenerator messageFunc)
        {
            if (_logger.IsErrorEnabled)
                LogMessage(LogLevel.Error, methodInfo, null, messageFunc());
        }

        public void LogErrorException(string methodInfo, string message, Exception exception)
        {
            if (_logger.IsErrorEnabled)
                LogMessage(LogLevel.Error, methodInfo, exception, message);
        }

        public void LogError(string methodInfo, Exception exception, string message)
        {
            if (_logger.IsErrorEnabled)
                LogMessage(LogLevel.Error, methodInfo, exception, message);
        }

        public void LogError(string methodInfo, Exception exception, string message, params object[] args)
        {
            if (_logger.IsErrorEnabled)
                LogMessage(LogLevel.Error, methodInfo, exception, message, null, args);
        }

        public void LogError(string methodInfo, Exception exception, IFormatProvider formatProvider, string message,
            params object[] args)
        {
            if (_logger.IsErrorEnabled)
                LogMessage(LogLevel.Error, methodInfo, exception, message, formatProvider, args);
        }

        public void LogError(string methodInfo, IFormatProvider formatProvider, string message, params object[] args)
        {
            if (_logger.IsErrorEnabled)
                LogMessage(LogLevel.Error, methodInfo, null, message, formatProvider, args);
        }

        public void LogError(string methodInfo, string message)
        {
            if (_logger.IsErrorEnabled)
                LogMessage(LogLevel.Error, methodInfo, null, message);
        }

        public void LogError(string methodInfo, string message, params object[] args)
        {
            if (_logger.IsErrorEnabled)
                LogMessage(LogLevel.Error, methodInfo, null, message, null, args);
        }

        public void LogError(string methodInfo, string message, Exception exception)
        {
            if (_logger.IsErrorEnabled)
                LogMessage(LogLevel.Error, methodInfo, exception, message);
        }

        public void LogError<TArgument>(string methodInfo, IFormatProvider formatProvider, string message,
            TArgument argument)
        {
            if (_logger.IsErrorEnabled)
                LogMessage(LogLevel.Error, methodInfo, null, message, formatProvider, new object[] { argument });
        }

        public void LogError<TArgument>(string methodInfo, string message, TArgument argument)
        {
            if (_logger.IsErrorEnabled)
                LogMessage(LogLevel.Error, methodInfo, null, message, null, new object[] { argument });
        }

        public void LogError<TArgument1, TArgument2>(string methodInfo, IFormatProvider formatProvider, string message,
            TArgument1 argument1, TArgument2 argument2)
        {
            if (_logger.IsErrorEnabled)
                LogMessage(LogLevel.Error, methodInfo, null, message, formatProvider, new object[] { argument1, argument2 });
        }

        public void LogError<TArgument1, TArgument2>(string methodInfo, string message, TArgument1 argument1,
            TArgument2 argument2)
        {
            if (_logger.IsErrorEnabled)
                LogMessage(LogLevel.Error, methodInfo, null, message, null, new object[] { argument1, argument2 });
        }

        public void LogError<TArgument1, TArgument2, TArgument3>(string methodInfo, IFormatProvider formatProvider,
            string message, TArgument1 argument1, TArgument2 argument2, TArgument3 argument3)
        {
            if (_logger.IsErrorEnabled)
                LogMessage(LogLevel.Error, methodInfo, null, message, formatProvider, new object[] { argument1, argument2, argument3 });
        }

        public void LogError<TArgument1, TArgument2, TArgument3>(string methodInfo, string message, TArgument1 argument1,
            TArgument2 argument2, TArgument3 argument3)
        {
            if (_logger.IsErrorEnabled)
                LogMessage(LogLevel.Error, methodInfo, null, message, null, new object[] { argument1, argument2, argument3 });
        }


        #endregion

        #region Fatal() overloads 

        public void LogFatal<T>(string methodInfo, T value)
        {
            if (_logger.IsFatalEnabled)
                LogValue(LogLevel.Fatal, methodInfo, value);
        }

        public void LogFatal<T>(string methodInfo, IFormatProvider formatProvider, T value)
        {
            if (_logger.IsFatalEnabled)
                LogValue(LogLevel.Fatal, methodInfo, value, formatProvider);
        }

        public void LogFatal(string methodInfo, LogMessageGenerator messageFunc)
        {
            if (_logger.IsFatalEnabled)
                LogMessage(LogLevel.Fatal, methodInfo, null, messageFunc());
        }

        public void LogFatalException(string methodInfo, string message, Exception exception)
        {
            if (_logger.IsFatalEnabled)
                LogMessage(LogLevel.Fatal, methodInfo, exception, message);
        }

        public void LogFatal(string methodInfo, Exception exception, string message)
        {
            if (_logger.IsFatalEnabled)
                LogMessage(LogLevel.Fatal, methodInfo, exception, message);
        }

        public void LogFatal(string methodInfo, Exception exception, string message, params object[] args)
        {
            if (_logger.IsFatalEnabled)
                LogMessage(LogLevel.Fatal, methodInfo, exception, message, null, args);
        }

        public void LogFatal(string methodInfo, Exception exception, IFormatProvider formatProvider, string message,
            params object[] args)
        {
            if (_logger.IsFatalEnabled)
                LogMessage(LogLevel.Fatal, methodInfo, exception, message, formatProvider, args);
        }

        public void LogFatal(string methodInfo, IFormatProvider formatProvider, string message, params object[] args)
        {
            if (_logger.IsFatalEnabled)
                LogMessage(LogLevel.Fatal, methodInfo, null, message, formatProvider, args);
        }

        public void LogFatal(string methodInfo, string message)
        {
            if (_logger.IsFatalEnabled)
                LogMessage(LogLevel.Fatal, methodInfo, null, message);
        }

        public void LogFatal(string methodInfo, string message, params object[] args)
        {
            if (_logger.IsFatalEnabled)
                LogMessage(LogLevel.Fatal, methodInfo, null, message, null, args);
        }

        public void LogFatal(string methodInfo, string message, Exception exception)
        {
            if (_logger.IsFatalEnabled)
                LogMessage(LogLevel.Fatal, methodInfo, exception, message);
        }

        public void LogFatal<TArgument>(string methodInfo, IFormatProvider formatProvider, string message,
            TArgument argument)
        {
            if (_logger.IsFatalEnabled)
                LogMessage(LogLevel.Fatal, methodInfo, null, message, formatProvider, new object[] { argument });
        }

        public void LogFatal<TArgument>(string methodInfo, string message, TArgument argument)
        {
            if (_logger.IsFatalEnabled)
                LogMessage(LogLevel.Fatal, methodInfo, null, message, null, new object[] { argument });
        }

        public void LogFatal<TArgument1, TArgument2>(string methodInfo, IFormatProvider formatProvider, string message,
            TArgument1 argument1, TArgument2 argument2)
        {
            if (_logger.IsFatalEnabled)
                LogMessage(LogLevel.Fatal, methodInfo, null, message, formatProvider, new object[] { argument1, argument2 });
        }

        public void LogFatal<TArgument1, TArgument2>(string methodInfo, string message, TArgument1 argument1,
            TArgument2 argument2)
        {
            if (_logger.IsFatalEnabled)
                LogMessage(LogLevel.Fatal, methodInfo, null, message, null, new object[] { argument1, argument2 });
        }

        public void LogFatal<TArgument1, TArgument2, TArgument3>(string methodInfo, IFormatProvider formatProvider,
            string message, TArgument1 argument1, TArgument2 argument2, TArgument3 argument3)
        {
            if (_logger.IsFatalEnabled)
                LogMessage(LogLevel.Fatal, methodInfo, null, message, formatProvider, new object[] { argument1, argument2, argument3 });
        }

        public void LogFatal<TArgument1, TArgument2, TArgument3>(string methodInfo, string message, TArgument1 argument1,
            TArgument2 argument2, TArgument3 argument3)
        {
            if (_logger.IsFatalEnabled)
                LogMessage(LogLevel.Fatal, methodInfo, null, message, null, new object[] { argument1, argument2, argument3 });
        }


        #endregion
    }
}
