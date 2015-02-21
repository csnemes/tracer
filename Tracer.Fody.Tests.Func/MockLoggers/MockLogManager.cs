using System;
using System.Collections.Generic;

namespace Tracer.Fody.Tests.Func.MockLoggers
{
    public class MockLogManager
    {
        private static readonly List<MockCallInfo> Calls = new List<MockCallInfo>();

        public static IMockLog GetLogger(Type type)
        {
            return new MockLog(type);
        }

        public static MockLogResult GetResult()
        {
            return new MockLogResult(Calls, true);
        }

        public static void TraceEnterCalled(string loggerName, string methodCalled, string[] paramNames, string[] paramValues)
        {
            Calls.Add(MockCallInfo.CreateEnter(loggerName, methodCalled, paramNames, paramValues));
        }

        public static void TraceLeaveCalled(string loggerName, string methodCalled, string returnValue)
        {
            Calls.Add(MockCallInfo.CreateLeave(loggerName, methodCalled, returnValue));
        }
    }
}