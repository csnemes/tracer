using System;

namespace Tracer.NLog4Net
{
    public static class Log 
    {
        public static void Debug(object message) { }
        public static void Debug(object message, Exception exception) { }
        public static void DebugFormat(string format, params object[] args) { }
        public static void DebugFormat(string format, object arg0) { }
        public static void DebugFormat(string format, object arg0, object arg1) { }
        public static void DebugFormat(string format, object arg0, object arg1, object arg2) { }
        public static void DebugFormat(IFormatProvider provider, string format, params object[] args) { }
        public static void Info(object message) { }
        public static void Info(object message, Exception exception){ }
        public static void InfoFormat(string format, params object[] args){ }
        public static void InfoFormat(string format, object arg0){ }
        public static void InfoFormat(string format, object arg0, object arg1){ }
        public static void InfoFormat(string format, object arg0, object arg1, object arg2){ }
        public static void InfoFormat(IFormatProvider provider, string format, params object[] args){ }
        public static void Warn(object message){ }
        public static void Warn(object message, Exception exception){ }
        public static void WarnFormat(string format, params object[] args){ }
        public static void WarnFormat(string format, object arg0){ }
        public static void WarnFormat(string format, object arg0, object arg1){ }
        public static void WarnFormat(string format, object arg0, object arg1, object arg2){ }
        public static void WarnFormat(IFormatProvider provider, string format, params object[] args){ }
        public static void Error(object message){ }
        public static void Error(object message, Exception exception){ }
        public static void ErrorFormat(string format, params object[] args){ }
        public static void ErrorFormat(string format, object arg0){ }
        public static void ErrorFormat(string format, object arg0, object arg1){ }
        public static void ErrorFormat(string format, object arg0, object arg1, object arg2){ }
        public static void ErrorFormat(IFormatProvider provider, string format, params object[] args){ }
        public static void Fatal(object message){ }
        public static void Fatal(object message, Exception exception){ }
        public static void FatalFormat(string format, params object[] args){ }
        public static void FatalFormat(string format, object arg0){ }
        public static void FatalFormat(string format, object arg0, object arg1){ }
        public static void FatalFormat(string format, object arg0, object arg1, object arg2){ }
        public static void FatalFormat(IFormatProvider provider, string format, params object[] args){ }
        public static bool IsDebugEnabled { get; }
        public static bool IsInfoEnabled { get; }
        public static bool IsWarnEnabled { get; }
        public static bool IsErrorEnabled { get; }
        public static bool IsFatalEnabled { get; }
    }
}
