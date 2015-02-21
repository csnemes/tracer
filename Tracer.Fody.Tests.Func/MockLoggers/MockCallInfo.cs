using System;
using System.Linq;
using FluentAssertions;

namespace Tracer.Fody.Tests.Func.MockLoggers
{
    [Serializable]
    public class MockCallInfo
    {
        [Serializable]
        public enum MockCallType
        {
            TraceEnter = 1,
            TraceLeave = 2
        }


        private readonly MockCallType _callType;
        private readonly string _loggerName;
        private readonly string _method;
        private readonly string _returnValue;
        private readonly string[] _paramNames;
        private readonly string[] _paramValues;


        private MockCallInfo(string loggerName, MockCallType callType, string method, string returnValue, string[] paramNames, string[] paramValues)
        {
            _loggerName = loggerName;
            _callType = callType;
            _method = method;
            _returnValue = returnValue;
            _paramNames = paramNames;
            _paramValues = paramValues;
        }

        public static MockCallInfo CreateEnter(string loggerName, string method, string[] paramNames = null, string[] paramValues = null)
        {
            return new MockCallInfo(loggerName, MockCallType.TraceEnter, method, null, paramNames, paramValues);
        }

        public static MockCallInfo CreateLeave(string loggerName, string method, string returnValue = null)
        {
            return new MockCallInfo(loggerName, MockCallType.TraceLeave, method, returnValue, null, null);
        }

        public string LoggerName
        {
            get { return _loggerName; }
        }

        public MockCallInfo.MockCallType CallType
        {
            get { return _callType; }
        }

        public string Method
        {
            get { return _method; }
        }

        public string ReturnValue
        {
            get { return _returnValue; }
        }

        public string[] ParamNames
        {
            get { return _paramNames; }
        }

        public string[] ParamValues
        {
            get { return _paramValues; }
        }
    }

    public static class MockCallInfoExtensions
    {
        public static void ShouldBeTraceEnterInto(this MockCallInfo mock, string methodFullName, params string[] parameters)
        {
            var split = methodFullName.Split(new [] {"::"}, StringSplitOptions.RemoveEmptyEntries);
            mock.LoggerName.Should().Be(split[0]);
            mock.Method.Should().Contain(split[1]);
            mock.CallType.Should().Be(MockCallInfo.MockCallType.TraceEnter);
            if (parameters != null)
            {
                for (int idx = 0; idx < parameters.Length; idx++)
                {
                    int mockIdx = idx/2;
                    if (idx%2 == 0)
                    {
                        mock.ParamNames[mockIdx].Should().Be(parameters[idx]);
                    }
                    else
                    {
                        mock.ParamValues[mockIdx].Should().Be(parameters[idx]);
                    }
                }
            }
        }

        public static void ShouldBeTraceLeaveFrom(this MockCallInfo mock, string methodFullName)
        {
            var split = methodFullName.Split(new[] { "::" }, StringSplitOptions.RemoveEmptyEntries);
            mock.LoggerName.Should().Be(split[0]);
            mock.Method.Should().Contain(split[1]);
            mock.CallType.Should().Be(MockCallInfo.MockCallType.TraceLeave);
        }

        public static void ShouldBeTraceLeaveFrom(this MockCallInfo mock, string methodFullName, string returnValue)
        {
            var split = methodFullName.Split(new[] { "::" }, StringSplitOptions.RemoveEmptyEntries);
            mock.LoggerName.Should().Be(split[0]);
            mock.Method.Should().Contain(split[1]);
            mock.CallType.Should().Be(MockCallInfo.MockCallType.TraceLeave);
            mock.ReturnValue.Should().Be(returnValue);
        }

    }
}