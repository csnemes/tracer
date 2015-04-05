using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tracer.Fody.Tests.Func.MockLoggers
{
    public class MockLogAdapter : IMockLogAdapter
    {
        private readonly Type _type;

        public MockLogAdapter(Type type)
        {
            _type = type;
        }

        public void TraceEnter(string methodInfo, string[] paramNames, object[] paramValues)
        {
            if (paramNames != null)
            {
                var stringValues = paramValues.Select(val => val != null ? val.ToString() : null).ToArray();
                MockLogManagerAdapter.TraceEnterCalled(_type.FullName, methodInfo, paramNames, stringValues);
            }
            else
            {
                MockLogManagerAdapter.TraceEnterCalled(_type.FullName, methodInfo, null, null);
            }
        }

        public void TraceLeave(string methodInfo, long numberOfTicks)
        {
            MockLogManagerAdapter.TraceLeaveCalled(_type.FullName, methodInfo, numberOfTicks, null);
        }

        public void TraceLeave(string methodInfo, long numberOfTicks, object returnValue)
        {
            var returnValueString = returnValue != null ? returnValue.ToString() : null;
            MockLogManagerAdapter.TraceLeaveCalled(_type.FullName, methodInfo, numberOfTicks, returnValueString);
        }

        public void MockLogOuter(string methodInfo, string message)
        {
            MockLogManagerAdapter.LogCalled(_type.FullName, methodInfo, "MockLogOuter",  new []{ message});
        }

        public void MockLogOuterNoParam(string methodInfo)
        {
            MockLogManagerAdapter.LogCalled(_type.FullName, methodInfo, "MockLogOuterNoParam");
        }

        public void MockLogOuter(string methodInfo, string message, int i)
        {
            MockLogManagerAdapter.LogCalled(_type.FullName, methodInfo, "MockLogOuter", new[] { message, i.ToString() });
        }
    }
}
