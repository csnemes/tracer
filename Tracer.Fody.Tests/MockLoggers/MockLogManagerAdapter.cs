using System;
using System.Collections.Generic;

namespace Tracer.Fody.Tests.MockLoggers
{
    public class MockLogManagerAdapter
    {
        private static readonly List<MockCallInfo> Calls = new List<MockCallInfo>();

        public static MockLogAdapter GetLogger(Type type)
        {
            return new MockLogAdapter(type);
        }

        public static MockLogResult GetResult()
        {
            return new MockLogResult(Calls, true);
        }

        public static void TraceEnterCalled(string loggerName, string containingMethod, string[] paramNames, string[] paramValues)
        {
            Calls.Add(MockCallInfo.CreateEnter(loggerName, containingMethod, paramNames, paramValues));
        }

        public static void TraceLeaveCalled(string loggerName, string containingMethod, long numberOfTicks, string[] paramNames, string[] paramValues)
        {
            Calls.Add(MockCallInfo.CreateLeave(loggerName, containingMethod, numberOfTicks, paramNames, paramValues));
        }

        public static void LogCalled(string loggerName, string containingMethod, string logMethodCalled,
             string[] paramValues = null)
        {
            Calls.Add(MockCallInfo.CreateLog(loggerName, containingMethod, logMethodCalled, paramValues));
        }

        public static void LogPropertyCalled(string loggerName, string logPropertyCalled)
        {
            Calls.Add(MockCallInfo.CreatePropertyAccess(loggerName, logPropertyCalled));
        }
    }
}