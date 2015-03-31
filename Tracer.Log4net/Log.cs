using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tracer.Log4net
{
    public static class Log
    {
        public static void ErrorEvent(Exception exception) {}
        public static void ErrorEvent(object message) {}
        public static void ErrorEvent(object message, Exception exception) {}
        public static void WarningEvent(Exception exception) {}
        public static void WarningEvent(object message) {}
        public static void WarningEvent(object message, Exception exception) {}
        public static void WarningEvent(string format, params object[] paramInfo) {}
        public static void InfoEvent(object message) {}
        public static void InfoEvent(string format, params object[] paramInfo) {}
        public static void DebugEvent(object message) {}
        public static void DebugEvent(string format, params object[] paramInfo) {}
        public static void TraceInfoEvent() {}
        public static void TraceInfoEvent(object message) {}
        public static void TraceInfoEvent(string format, params object[] paramInfo) {}
        public static void VerboseEvent(object message) {}
        public static void VerboseEvent(string format, params object[] paramInfo) {}
    }
}
