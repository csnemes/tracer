using FluentAssertions;
using NUnit.Framework;
using Tracer.Fody.Tests.Func.MockLoggers;

namespace Tracer.Fody.Tests.Func.TraceTests
{
    [TestFixture]
    public class NoParamsRetvalTests : FuncTestBase
    {
        [Test]
        public void Test_Static_Empty_Method_Returns_Int()
        {
            string code = @"
                using System;
                using System.Diagnostics;

                namespace First
                {
                    public class MyClass
                    {
                        public static void Main()
                        {
                            var i = GetIntValue();
                        }

                        private static int GetIntValue()
                        {
                            return 1;
                        }
                    }
                }
            ";

            var result = this.RunTest(code, new PrivateOnlyTraceLoggingFilter(), "First.MyClass::Main");
            result.Count.Should().Be(2);
            result.ElementAt(0).ShouldBeTraceEnterInto("First.MyClass::GetIntValue");
            result.ElementAt(1).ShouldBeTraceLeaveFrom("First.MyClass::GetIntValue", "1");
        }

        [Test]
        public void Test_Static_Empty_Method_Returns_String()
        {
            string code = @"
                using System;
                using System.Diagnostics;

                namespace First
                {
                    public class MyClass
                    {
                        public static void Main()
                        {
                            var str = GetStringValue();
                        }

                        private static string GetStringValue()
                        {
                            return ""Hello"";
                        }
                    }
                }
            ";

            var result = this.RunTest(code, new PrivateOnlyTraceLoggingFilter(), "First.MyClass::Main");
            result.Count.Should().Be(2);
            result.ElementAt(0).ShouldBeTraceEnterInto("First.MyClass::GetStringValue");
            result.ElementAt(1).ShouldBeTraceLeaveFrom("First.MyClass::GetStringValue", "Hello");
        }
    }
}
