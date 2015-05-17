using FluentAssertions;
using NUnit.Framework;
using Tracer.Fody.Tests.MockLoggers;

namespace Tracer.Fody.Tests.TraceTests
{
    [TestFixture]
    public class NoParamsRetvalTests : TestBase
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

        [Test]
        public void Test_Static_Empty_Method_Returns_Object()
        {
            string code = @"
                using System;
                using System.Diagnostics;

                namespace First
                {
                    public class MyRetVal
                    {}

                    public class MyClass
                    {
                        public static void Main()
                        {
                            var val = GetValue();
                        }

                        private static MyRetVal GetValue()
                        {
                            return new MyRetVal();
                        }
                    }
                }
            ";

            var result = this.RunTest(code, new PrivateOnlyTraceLoggingFilter(), "First.MyClass::Main");
            result.Count.Should().Be(2);
            result.ElementAt(0).ShouldBeTraceEnterInto("First.MyClass::GetValue");
            result.ElementAt(1).ShouldBeTraceLeaveFrom("First.MyClass::GetValue", "First.MyRetVal");
        }

        [Test]
        public void Test_Static_Empty_Method_Returns_Struct()
        {
            string code = @"
                using System;
                using System.Diagnostics;

                namespace First
                {
                    public struct MyStruct
                    {
                        public int IntVal { get; set; }

                        public override string ToString()
                        {
                            return String.Format(""I{0}"", IntVal);
                        }
                    }

                    public class MyClass
                    {
                        public static void Main()
                        {
                            var i = GetStructValue();
                        }

                        private static MyStruct GetStructValue()
                        {
                            return new MyStruct() { IntVal = 42 };
                        }
                    }
                }
            ";

            var result = this.RunTest(code, new PrivateOnlyTraceLoggingFilter(), "First.MyClass::Main");
            result.Count.Should().Be(2);
            result.ElementAt(0).ShouldBeTraceEnterInto("First.MyClass::GetStructValue");
            result.ElementAt(1).ShouldBeTraceLeaveFrom("First.MyClass::GetStructValue", "I42");
        }

        [Test]
        public void Test_Instance_Empty_Method_Returns_Int()
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
                            var mc = new MyClass(); 
                            var i = mc.GetIntValue();
                        }

                        private int GetIntValue()
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
        public void Test_Instance_Empty_Method_Returns_Struct()
        {
            string code = @"
                using System;
                using System.Diagnostics;

                namespace First
                {
                    public struct MyStruct
                    {
                        public int IntVal { get; set; }

                        public override string ToString()
                        {
                            return String.Format(""I{0}"", IntVal);
                        }
                    }

                    public class MyClass
                    {
                        public static void Main()
                        {
                            var mc = new MyClass();
                            var i = mc.GetStructValue();
                        }

                        private MyStruct GetStructValue()
                        {
                            return new MyStruct() { IntVal = 42 };
                        }
                    }
                }
            ";

            var result = this.RunTest(code, new PrivateOnlyTraceLoggingFilter(), "First.MyClass::Main");
            result.Count.Should().Be(2);
            result.ElementAt(0).ShouldBeTraceEnterInto("First.MyClass::GetStructValue");
            result.ElementAt(1).ShouldBeTraceLeaveFrom("First.MyClass::GetStructValue", "I42");
        }
    }
}
