using FluentAssertions;
using NUnit.Framework;
using Tracer.Fody.Tests.Func.MockLoggers;

namespace Tracer.Fody.Tests.Func.TraceTests
{
    [TestFixture]
    public class ParamsNoRetvalTests : FuncTestBase
    {
        [Test]
        public void Test_Static_SingleStringParameter_Empty_Method()
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
                            SetString(""Hello"");
                        }

                        private static void SetString(string param)
                        {
                        }
                    }
                }
            ";

            var result = this.RunTest(code, new PrivateOnlyTraceLoggingFilter(), "First.MyClass::Main");
            result.Count.Should().Be(2);
            result.ElementAt(0).ShouldBeTraceEnterInto("First.MyClass::SetString", "param", "Hello");
            result.ElementAt(1).ShouldBeTraceLeaveFrom("First.MyClass::SetString");
        }

        [Test]
        public void Test_Static_SingleIntParameter_Empty_Method()
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
                            SetInt(42);
                        }

                        private static void SetInt(int param)
                        {
                        }
                    }
                }
            ";

            var result = this.RunTest(code, new PrivateOnlyTraceLoggingFilter(), "First.MyClass::Main");
            result.Count.Should().Be(2);
            result.ElementAt(0).ShouldBeTraceEnterInto("First.MyClass::SetInt", "param", "42");
            result.ElementAt(1).ShouldBeTraceLeaveFrom("First.MyClass::SetInt");
        }

        [Test]
        public void Test_Static_OutParameter_NotListed()
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
                            string outval;
                            SetString(""Hello"", out outval);
                        }

                        private static void SetString(string param, out string outval)
                        {
                            outval = ""outval"";
                        }
                    }
                }
            ";

            var result = this.RunTest(code, new PrivateOnlyTraceLoggingFilter(), "First.MyClass::Main");
            result.Count.Should().Be(2);
            result.ElementAt(0).ShouldBeTraceEnterInto("First.MyClass::SetString", "param", "Hello");
            result.ElementAt(1).ShouldBeTraceLeaveFrom("First.MyClass::SetString");
        }

        [Test]
        public void Test_Static_Method_Multiple_Returns()
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
                            Run(1);
                            Run(0);
                            Run(-1);
                        }

                        private static void Run(int input)
                        {
                            if (input > 0)
                            {
                                return;
                            }    
                            input = input + 1;
                            if (input > 0)
                            {
                                return;
                            }
                        }
                    }
                }
            ";

            var result = this.RunTest(code, new PrivateOnlyTraceLoggingFilter(), "First.MyClass::Main");
            result.Count.Should().Be(6);
            result.ElementAt(0).ShouldBeTraceEnterInto("First.MyClass::Run", "input", "1");
            result.ElementAt(1).ShouldBeTraceLeaveFrom("First.MyClass::Run");
            result.ElementAt(2).ShouldBeTraceEnterInto("First.MyClass::Run", "input", "0");
            result.ElementAt(3).ShouldBeTraceLeaveFrom("First.MyClass::Run");
            result.ElementAt(4).ShouldBeTraceEnterInto("First.MyClass::Run", "input", "-1");
            result.ElementAt(5).ShouldBeTraceLeaveFrom("First.MyClass::Run");
        }
    }
}
