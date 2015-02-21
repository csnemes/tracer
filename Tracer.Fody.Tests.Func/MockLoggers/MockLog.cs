using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tracer.Fody.Tests.Func.MockLoggers
{
    public class MockLog : IMockLog
    {
        private readonly Type _type;

        public MockLog(Type type)
        {
            _type = type;
        }

        public void TraceEnter(string methodInfo)
        {
            MockLogManager.TraceEnterCalled(_type.FullName, methodInfo, null, null);
        }

        public void TraceEnter(string methodInfo, string[] paramNames, object[] paramValues)
        {
            var stringValues = paramValues.Select(val => val != null ? val.ToString() : null).ToArray();
            MockLogManager.TraceEnterCalled(_type.FullName, methodInfo, paramNames, stringValues);
        }

        public void TraceLeave(string methodInfo, long numberOfTicks)
        {
            MockLogManager.TraceLeaveCalled(_type.FullName, methodInfo, null);
        }

        public void TraceLeave(string methodInfo, long numberOfTicks, object returnValue)
        {
            var returnValueString = returnValue != null ? returnValue.ToString() : null;
            MockLogManager.TraceLeaveCalled(_type.FullName, methodInfo, returnValueString);
        }
    }
}
