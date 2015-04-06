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
        public void Test_Static_ObjectParameter()
        {
            string code = @"
                using System;
                using System.Diagnostics;

                namespace First
                {
                    public class MyInput
                    {}

                    public class MyClass
                    {
                        public static void Main()
                        {
                            CallMe(new MyInput());
                        }

                        private static void CallMe(MyInput param)
                        {
                        }
                    }
                }
            ";

            var result = this.RunTest(code, new PrivateOnlyTraceLoggingFilter(), "First.MyClass::Main");
            result.Count.Should().Be(2);
            result.ElementAt(0).ShouldBeTraceEnterInto("First.MyClass::CallMe", "param", "First.MyInput");
            result.ElementAt(1).ShouldBeTraceLeaveFrom("First.MyClass::CallMe");
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
        public void Test_Static_MultipleParameters()
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
                            CallMe(""Hello"", ""Hello2"", 42);
                        }

                        private static void CallMe(string param, string param2, int paraInt)
                        {
                        }
                    }
                }
            ";

            var result = this.RunTest(code, new PrivateOnlyTraceLoggingFilter(), "First.MyClass::Main");
            result.Count.Should().Be(2);
            result.ElementAt(0).ShouldBeTraceEnterInto("First.MyClass::CallMe", "param", "Hello", "param2", "Hello2", "paraInt", "42");
            result.ElementAt(1).ShouldBeTraceLeaveFrom("First.MyClass::CallMe");
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

        [Test]
        public void Test_Static_SingleStructParameter_Empty_Method()
        {
            string code = @"
                using System;
                using System.Diagnostics;

                namespace First
                {
                    public struct MyStruct
                    {
                        public string ValStr {get; set;}
                        public int ValInt {get; set;}
                    }

                    public class MyClass
                    {
                        public static void Main()
                        {
                            SetStruct(new MyStruct() { ValStr=""Hi"", ValInt=42 });
                        }

                        private static void SetStruct(MyStruct param)
                        {
                        }
                    }
                }
            ";

            var result = this.RunTest(code, new PrivateOnlyTraceLoggingFilter(), "First.MyClass::Main");
            result.Count.Should().Be(2);
            result.ElementAt(0).ShouldBeTraceEnterInto("First.MyClass::SetStruct", "param", "First.MyStruct");
            result.ElementAt(1).ShouldBeTraceLeaveFrom("First.MyClass::SetStruct");
        }

        [Test]
        public void Test_Instance_MultipleParameters()
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
                            mc.CallMe(""Hello"", ""Hello2"", 42);
                        }

                        private void CallMe(string param, string param2, int paraInt)
                        {
                        }
                    }
                }
            ";

            var result = this.RunTest(code, new PrivateOnlyTraceLoggingFilter(), "First.MyClass::Main");
            result.Count.Should().Be(2);
            result.ElementAt(0).ShouldBeTraceEnterInto("First.MyClass::CallMe", "param", "Hello", "param2", "Hello2", "paraInt", "42");
            result.ElementAt(1).ShouldBeTraceLeaveFrom("First.MyClass::CallMe");
        }


        [Test]
        public void Test_Instance_Method_Multiple_Returns()
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
                            mc.Run(1);
                            mc.Run(0);
                            mc.Run(-1);
                        }

                        private void Run(int input)
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
