using Serilog.Core;
using Serilog.Events;
using Serilog.Parsing;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using SL = Serilog;

namespace Tracer.Serilog.Adapters
{
    public class LoggerAdapter
    {
        private readonly SL.ILogger _logger;
        private readonly string _typeName;
        private const string NullString = "<NULL>";

        private static readonly MessageTemplate _traceEnterTemplate;
        private static readonly MessageTemplate _traceLeaveTemplate;
        private static readonly ConcurrentDictionary<Type, bool> _destructureFlags = new ConcurrentDictionary<Type, bool>();
        private static readonly ConcurrentDictionary<Assembly, byte> _assembliesParsedForDestructureTypeAttribute = new ConcurrentDictionary<Assembly, byte>(); //value doesnt really matter
        
        private readonly Func<object, string, string> _renderParameterMethod;

        static LoggerAdapter()
        {
            var parser = new MessageTemplateParser();
            _traceEnterTemplate = parser.Parse("Entered into {MethodName} ({CallingParameters}).");
            _traceLeaveTemplate = parser.Parse("Returned from {MethodName} ({ReturnValue}). Time taken: {TimeTaken:0.00} ms.");
        }

        public LoggerAdapter(Type type, bool logUseSafeParameterRendering=false)
        {
            _logger = SL.Log.Logger.ForContext(type);
            _typeName = PrettyFormat(type);

            _assembliesParsedForDestructureTypeAttribute.GetOrAdd(type.Assembly, SeekForDestructureTypeAttribute);

            //var config = ConfigurationManager.AppSettings["LogUseSafeParameterRendering"];
            

            if (logUseSafeParameterRendering)
                _renderParameterMethod = GetSafeRenderedFormat;
            else
                _renderParameterMethod = GetRenderedFormat;
        }

        public void LogWrite(string methodInfo, LogEventLevel level, string messageTemplate)
        {
            if (_logger.IsEnabled(level))
            {
                DoLog(methodInfo, level, null, messageTemplate, null);
            }
        }

        public void LogWrite(string methodInfo, LogEventLevel level, string messageTemplate, params object[] propertyValues)
        {
            if (_logger.IsEnabled(level))
            {
                DoLog(methodInfo, level, null, messageTemplate, propertyValues);
            }
        }

        public void LogWrite(string methodInfo, LogEventLevel level, Exception exception, string messageTemplate)
        {
            if (_logger.IsEnabled(level))
            {
                DoLog(methodInfo, level, exception, messageTemplate, null);
            }
        }

        public void LogWrite(string methodInfo, LogEventLevel level, Exception exception, string messageTemplate,
            params object[] propertyValues)
        {
            if (_logger.IsEnabled(level))
            {
                DoLog(methodInfo, level, exception, messageTemplate, propertyValues);
            }
        }

        public bool LogIsEnabled(string methodInfo, LogEventLevel level)
        {
            return _logger.IsEnabled(level);
        }

        public void LogVerbose(string methodInfo, string messageTemplate)
        {
            if (_logger.IsEnabled(LogEventLevel.Verbose))
            {
               DoLog(methodInfo, LogEventLevel.Verbose, null, messageTemplate, null);
            }
        }
        
        public void LogVerbose(string methodInfo, string messageTemplate, object[] parameters)
        {
            if (_logger.IsEnabled(LogEventLevel.Verbose))
            {
                DoLog(methodInfo, LogEventLevel.Verbose, null, messageTemplate, parameters);
            }
        }

        public void LogVerbose(string methodInfo, Exception exception, string messageTemplate)
        {
            if (_logger.IsEnabled(LogEventLevel.Verbose))
            {
                DoLog(methodInfo, LogEventLevel.Verbose, exception, messageTemplate, null);
            }
        }

        public void LogVerbose(string methodInfo, Exception exception, string messageTemplate, object[] parameters)
        {
            if (_logger.IsEnabled(LogEventLevel.Verbose))
            {
                DoLog(methodInfo, LogEventLevel.Verbose, exception, messageTemplate, parameters);
            }
        }

        public void LogDebug(string methodInfo, string messageTemplate)
        {
            if (_logger.IsEnabled(LogEventLevel.Debug))
            {
                DoLog(methodInfo, LogEventLevel.Debug, null, messageTemplate, null);
            }
        }

        public void LogDebug(string methodInfo, string messageTemplate, object[] parameters)
        {
            if (_logger.IsEnabled(LogEventLevel.Debug))
            {
                DoLog(methodInfo, LogEventLevel.Debug, null, messageTemplate, parameters);
            }
        }

        public void LogDebug(string methodInfo, Exception exception, string messageTemplate)
        {
            if (_logger.IsEnabled(LogEventLevel.Debug))
            {
                DoLog(methodInfo, LogEventLevel.Debug, exception, messageTemplate, null);
            }
        }

        public void LogDebug(string methodInfo, Exception exception, string messageTemplate, object[] parameters)
        {
            if (_logger.IsEnabled(LogEventLevel.Debug))
            {
                DoLog(methodInfo, LogEventLevel.Debug, exception, messageTemplate, parameters);
            }
        }

        public void LogInformation(string methodInfo, string messageTemplate)
        {
            if (_logger.IsEnabled(LogEventLevel.Information))
            {
                DoLog(methodInfo, LogEventLevel.Information, null, messageTemplate, null);
            }
        }

        public void LogInformation(string methodInfo, string messageTemplate, object[] parameters)
        {
            if (_logger.IsEnabled(LogEventLevel.Information))
            {
                DoLog(methodInfo, LogEventLevel.Information, null, messageTemplate, parameters);
            }
        }

        public void LogInformation(string methodInfo, Exception exception, string messageTemplate)
        {
            if (_logger.IsEnabled(LogEventLevel.Information))
            {
                DoLog(methodInfo, LogEventLevel.Information, exception, messageTemplate, null);
            }
        }

        public void LogInformation(string methodInfo, Exception exception, string messageTemplate, object[] parameters)
        {
            if (_logger.IsEnabled(LogEventLevel.Information))
            {
                DoLog(methodInfo, LogEventLevel.Information, exception, messageTemplate, parameters);
            }
        }

        public void LogWarning(string methodInfo, string messageTemplate)
        {
            if (_logger.IsEnabled(LogEventLevel.Warning))
            {
                DoLog(methodInfo, LogEventLevel.Warning, null, messageTemplate, null);
            }
        }

        public void LogWarning(string methodInfo, string messageTemplate, object[] parameters)
        {
            if (_logger.IsEnabled(LogEventLevel.Warning))
            {
                DoLog(methodInfo, LogEventLevel.Warning, null, messageTemplate, parameters);
            }
        }

        public void LogWarning(string methodInfo, Exception exception, string messageTemplate)
        {
            if (_logger.IsEnabled(LogEventLevel.Warning))
            {
                DoLog(methodInfo, LogEventLevel.Warning, exception, messageTemplate, null);
            }
        }

        public void LogWarning(string methodInfo, Exception exception, string messageTemplate, object[] parameters)
        {
            if (_logger.IsEnabled(LogEventLevel.Warning))
            {
                DoLog(methodInfo, LogEventLevel.Warning, exception, messageTemplate, parameters);
            }
        }

        public void LogError(string methodInfo, string messageTemplate)
        {
            if (_logger.IsEnabled(LogEventLevel.Error))
            {
                DoLog(methodInfo, LogEventLevel.Error, null, messageTemplate, null);
            }
        }

        public void LogError(string methodInfo, string messageTemplate, object[] parameters)
        {
            if (_logger.IsEnabled(LogEventLevel.Error))
            {
                DoLog(methodInfo, LogEventLevel.Error, null, messageTemplate, parameters);
            }
        }

        public void LogError(string methodInfo, Exception exception, string messageTemplate)
        {
            if (_logger.IsEnabled(LogEventLevel.Error))
            {
                DoLog(methodInfo, LogEventLevel.Error, exception, messageTemplate, null);
            }
        }

        public void LogError(string methodInfo, Exception exception, string messageTemplate, object[] parameters)
        {
            if (_logger.IsEnabled(LogEventLevel.Error))
            {
                DoLog(methodInfo, LogEventLevel.Error, exception, messageTemplate, parameters);
            }
        }

        public void LogFatal(string methodInfo, string messageTemplate)
        {
            if (_logger.IsEnabled(LogEventLevel.Fatal))
            {
                DoLog(methodInfo, LogEventLevel.Fatal, null, messageTemplate, null);
            }
        }

        public void LogFatal(string methodInfo, string messageTemplate, object[] parameters)
        {
            if (_logger.IsEnabled(LogEventLevel.Fatal))
            {
                DoLog(methodInfo, LogEventLevel.Fatal, null, messageTemplate, parameters);
            }
        }

        public void LogFatal(string methodInfo, Exception exception, string messageTemplate)
        {
            if (_logger.IsEnabled(LogEventLevel.Fatal))
            {
                DoLog(methodInfo, LogEventLevel.Fatal, exception, messageTemplate, null);
            }
        }

        public void LogFatal(string methodInfo, Exception exception, string messageTemplate, object[] parameters)
        {
            if (_logger.IsEnabled(LogEventLevel.Fatal))
            {
                DoLog(methodInfo, LogEventLevel.Fatal, exception, messageTemplate, parameters);
            }
        }

        [MessageTemplateFormatMethod("messageTemplate")]
        private void DoLog(string methodName, LogEventLevel level, Exception exception, string messageTemplate,  object[] propertyValues)
        {
            if (propertyValues != null &&
                propertyValues.GetType() != typeof(object[]))
                propertyValues = new object[] { propertyValues };

            MessageTemplate parsedTemplate;
            IEnumerable<LogEventProperty> boundProperties;
            _logger.BindMessageTemplate(messageTemplate, propertyValues, out parsedTemplate, out boundProperties);

            var logEvent = new LogEvent(DateTimeOffset.Now, level, exception, parsedTemplate, boundProperties);

            logEvent.AddPropertyIfAbsent(new LogEventProperty("MethodName", new ScalarValue(methodName)));
            logEvent.AddPropertyIfAbsent(new LogEventProperty("ClassName", new ScalarValue(_typeName)));

            _logger.Write(logEvent);
        }

        public void TraceEnter(string methodName, string[] paramNames, object[] paramValues)
        {
            if (_logger.IsEnabled(LogEventLevel.Debug))
            {
                var props = new List<LogEventProperty>();

                if (paramNames != null)
                {
                    for (var i = 0; i < paramNames.Length; i++)
                    {
                        if (paramValues[i] != null && ShouldDestructure(paramValues[i].GetType()))
                        {
                            LogEventProperty prop;
                            if (_logger.BindProperty(paramNames[i], paramValues[i] ?? NullString, true, out prop))
                            {
                                props.Add(prop);
                            }
                        }
                        else
                        {
                            props.Add(new LogEventProperty(paramNames[i], new ScalarValue(_renderParameterMethod(paramValues[i], NullString))));
                        }
                    }
                }

                var properties = new List<LogEventProperty>
                {
                    new LogEventProperty("MethodName", new ScalarValue(methodName)),
                    new LogEventProperty("CallingParameters", new StructureValue(props))
                };

                var logEvent = new LogEvent(DateTimeOffset.Now, LogEventLevel.Debug, null, _traceEnterTemplate, properties);

                logEvent.AddPropertyIfAbsent(new LogEventProperty("TraceType", new ScalarValue("Enter")));
                logEvent.AddPropertyIfAbsent(new LogEventProperty("ClassName", new ScalarValue(_typeName)));

                _logger.Write(logEvent);
            }
        }

        public void TraceLeave(string methodName, long startTicks, long endTicks, string[] paramNames, object[] paramValues)
        {
            if (_logger.IsEnabled(LogEventLevel.Debug))
            {
                var props = new List<LogEventProperty>();

                if (paramNames != null)
                {
                    for (var i = 0; i < paramNames.Length; i++)
                    {
                        if (paramValues[i] != null && ShouldDestructure(paramValues[i].GetType()))
                        {
                            LogEventProperty prop;
                            if (_logger.BindProperty(paramNames[i] ?? "$return", paramValues[i] ?? NullString, true, out prop))
                            {
                                props.Add(prop);
                            }
                        }
                        else
                        {
                            props.Add(new LogEventProperty(paramNames[i] ?? "$return", new ScalarValue(_renderParameterMethod(paramValues[i], NullString))));
                        }
                    }
                }

                var timeTaken = ConvertTicksToMilliseconds(endTicks - startTicks);

                var properties = new List<LogEventProperty>
                {
                    new LogEventProperty("MethodName", new ScalarValue(methodName)),
                    new LogEventProperty("ReturnValue", new StructureValue(props)),
                    new LogEventProperty("TimeTaken", new ScalarValue(timeTaken)),
                };

                var logEvent = new LogEvent(DateTimeOffset.Now, LogEventLevel.Debug, null, _traceLeaveTemplate, properties);

                logEvent.AddPropertyIfAbsent(new LogEventProperty("StartTicks", new ScalarValue(startTicks)));
                logEvent.AddPropertyIfAbsent(new LogEventProperty("EndTicks", new ScalarValue(endTicks)));
                logEvent.AddPropertyIfAbsent(new LogEventProperty("TraceType", new ScalarValue("Leave")));
                logEvent.AddPropertyIfAbsent(new LogEventProperty("ClassName", new ScalarValue(_typeName)));

                _logger.Write(logEvent);
            }
        }

        private bool ShouldDestructure(Type type)
        {
            return _destructureFlags.GetOrAdd(type, it => it.GetCustomAttribute<DestructureAttribute>() != null);
        }

        private byte SeekForDestructureTypeAttribute(Assembly asm)
        {
            var attribs = asm.GetCustomAttributes(typeof(DestructureTypeAttribute), false).Cast<DestructureTypeAttribute>();
            foreach (var attrib in attribs)
            {
                _destructureFlags.AddOrUpdate(attrib.TypeToDestructure, it => true, (it, old) => true);
            }
            return 0;
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
            return ticks * (10000000 / (double)Stopwatch.Frequency) / 10000L;
        }

        private static string PrettyFormat(Type type)
        {
            var sb = new StringBuilder();
            if (type.IsGenericType && type.Name.Contains('`'))
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
                return (string)message;
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

    }
}
