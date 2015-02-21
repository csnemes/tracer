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
            TraceLeave = 2,
            Log = 3
        }


        private readonly MockCallType _callType;
        private readonly string _loggerName;
        private readonly string _containingMethod;
        private readonly string _returnValue;
        private readonly string[] _paramNames;
        private readonly string[] _paramValues;
        private readonly string _logMethod;

        private MockCallInfo(string loggerName, MockCallType callType, string containingMethod, string returnValue, string[] paramNames, string[] paramValues,
            string logMethod)
        {
            _loggerName = loggerName;
            _callType = callType;
            _containingMethod = containingMethod;
            _returnValue = returnValue;
            _paramNames = paramNames;
            _paramValues = paramValues;
            _logMethod = logMethod;
        }

        public static MockCallInfo CreateEnter(string loggerName, string containingMethod, string[] paramNames = null, string[] paramValues = null)
        {
            return new MockCallInfo(loggerName, MockCallType.TraceEnter, containingMethod, null, paramNames, paramValues, null);
        }

        public static MockCallInfo CreateLeave(string loggerName, string containingMethod, string returnValue = null)
        {
            return new MockCallInfo(loggerName, MockCallType.TraceLeave, containingMethod, returnValue, null, null, null);
        }

        public static MockCallInfo CreateLog(string loggerName, string containingMethod, string logMethod,
            string[] paramNames = null, string[] paramValues = null)
        {
            return new MockCallInfo(loggerName, MockCallType.Log, containingMethod, null, paramNames, paramValues, logMethod);
        }
        
        public string LoggerName
        {
            get { return _loggerName; }
        }

        public MockCallInfo.MockCallType CallType
        {
            get { return _callType; }
        }

        public string ContainingMethod
        {
            get { return _containingMethod; }
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

        public string LogMethod
        {
            get { return _logMethod; }
        }
    }

    public static class MockCallInfoExtensions
    {
        public static void ShouldBeTraceEnterInto(this MockCallInfo mock, string methodFullName, params string[] parameters)
        {
            var split = methodFullName.Split(new [] {"::"}, StringSplitOptions.RemoveEmptyEntries);
            mock.LoggerName.Should().Be(split[0]);
            mock.ContainingMethod.Should().Contain(split[1]);
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
            mock.ContainingMethod.Should().Contain(split[1]);
            mock.CallType.Should().Be(MockCallInfo.MockCallType.TraceLeave);
        }

        public static void ShouldBeTraceLeaveFrom(this MockCallInfo mock, string methodFullName, string returnValue)
        {
            var split = methodFullName.Split(new[] { "::" }, StringSplitOptions.RemoveEmptyEntries);
            mock.LoggerName.Should().Be(split[0]);
            mock.ContainingMethod.Should().Contain(split[1]);
            mock.CallType.Should().Be(MockCallInfo.MockCallType.TraceLeave);
            mock.ReturnValue.Should().Be(returnValue);
        }

    }
}