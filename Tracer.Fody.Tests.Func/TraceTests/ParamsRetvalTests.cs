using FluentAssertions;
using NUnit.Framework;
using Tracer.Fody.Tests.Func.MockLoggers;

namespace Tracer.Fody.Tests.Func.TraceTests
{
    [TestFixture]
    public class ParamsRetvalTests : FuncTestBase
    {
        [Test]
        public void Test_Static_MultipleParameters_Returns_String()
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
                            CallMe(""Ahoy"", ""Ahoy2"", 43);
                        }

                        private static string CallMe(string param, string param2, int paraInt)
                        {
                            return ""response"" + paraInt.ToString();
                        }
                    }
                }
            ";

            var result = this.RunTest(code, new PrivateOnlyTraceLoggingFilter(), "First.MyClass::Main");
            result.Count.Should().Be(4);
            result.ElementAt(0).ShouldBeTraceEnterInto("First.MyClass::CallMe", "param", "Hello", "param2", "Hello2", "paraInt", "42");
            result.ElementAt(1).ShouldBeTraceLeaveFrom("First.MyClass::CallMe", "response42");
            result.ElementAt(2).ShouldBeTraceEnterInto("First.MyClass::CallMe", "param", "Ahoy", "param2", "Ahoy2", "paraInt", "43");
            result.ElementAt(3).ShouldBeTraceLeaveFrom("First.MyClass::CallMe", "response43");
        }

        [Test]
        public void Test_Instance_MultipleParameters_Returns_Int()
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
                            var myClass = new MyClass();
                            myClass.CallMe(""Hello"", ""Hello2"", 42);
                            myClass.CallMe(""Ahoy"", ""Ahoy2"", 43);
                        }

                        private int CallMe(string param, string param2, int paraInt)
                        {
                            return paraInt * 2;
                        }
                    }
                }
            ";

            var result = this.RunTest(code, new PrivateOnlyTraceLoggingFilter(), "First.MyClass::Main");
            result.Count.Should().Be(4);
            result.ElementAt(0).ShouldBeTraceEnterInto("First.MyClass::CallMe", "param", "Hello", "param2", "Hello2", "paraInt", "42");
            result.ElementAt(1).ShouldBeTraceLeaveFrom("First.MyClass::CallMe", "84");
            result.ElementAt(2).ShouldBeTraceEnterInto("First.MyClass::CallMe", "param", "Ahoy", "param2", "Ahoy2", "paraInt", "43");
            result.ElementAt(3).ShouldBeTraceLeaveFrom("First.MyClass::CallMe", "86");
        }

        [Test]
        public void Test_Generic_Parameter_Method()
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
                            var myClass = new MyClass();
                            myClass.CallMe<string, int>(""Hello"", ""Hello2"", 42);
                            myClass.CallMe<string, double>(""Ahoy"", ""Ahoy2"", 0.5);
                        }

                        private string CallMe<T, K>(T param, string param2, K paraNum)
                        {
                            return param2 + ""!"";
                        }
                    }
                }
            ";

            var result = this.RunTest(code, new PrivateOnlyTraceLoggingFilter(), "First.MyClass::Main");
            result.Count.Should().Be(4);
            result.ElementAt(0).ShouldBeTraceEnterInto("First.MyClass::CallMe", "param", "Hello", "param2", "Hello2", "paraNum", "42");
            result.ElementAt(1).ShouldBeTraceLeaveFrom("First.MyClass::CallMe", "Hello2!");
            result.ElementAt(2).ShouldBeTraceEnterInto("First.MyClass::CallMe", "param", "Ahoy", "param2", "Ahoy2", "paraNum", "0,5");
            result.ElementAt(3).ShouldBeTraceLeaveFrom("First.MyClass::CallMe", "Ahoy2!");
        }

        [Test]
        public void Test_Generic_ReturnValue_Method()
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
                            var myClass = new MyClass();
                            var r1 = myClass.CallMe<string>(""Hello"");
                            var r2 = myClass.CallMe<int>(""Ahoy"");
                        }

                        private T CallMe<T>(string param)
                        {
                            return default(T);
                        }
                    }
                }
            ";

            var result = this.RunTest(code, new PrivateOnlyTraceLoggingFilter(), "First.MyClass::Main");
            result.Count.Should().Be(4);
            result.ElementAt(0).ShouldBeTraceEnterInto("First.MyClass::CallMe", "param", "Hello");
            result.ElementAt(1).ShouldBeTraceLeaveFrom("First.MyClass::CallMe", null);
            result.ElementAt(2).ShouldBeTraceEnterInto("First.MyClass::CallMe", "param", "Ahoy");
            result.ElementAt(3).ShouldBeTraceLeaveFrom("First.MyClass::CallMe", "0");
        }


        [Test, Explicit]
        //This one fails, incorrect IL code, check CECIL
        public void Test_Generic_Class_Method()
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
                            var myStringClass = new MyGenClass<string>();
                            var res1 = myStringClass.CallMe(""Hello"", ""John"");
                            var myIntClass = new MyGenClass<int>();
                            var res2 = myIntClass.CallMe(""Hello"", 42);
                        }
                    }

                    public class MyGenClass<T>
                    {
                        internal T CallMe(string param, T paramT)
                        {
                            return default(T);
                        }
                    }
                }
            ";

            var result = this.RunTest(code, new InternalOnlyTraceLoggingFilter(), "First.MyClass::Main");
            result.Count.Should().Be(4);
            result.ElementAt(0).ShouldBeTraceEnterInto("First.MyClass::CallMe", "param", "Hello", "param2", "Hello2", "paraNum", "42");
            result.ElementAt(1).ShouldBeTraceLeaveFrom("First.MyClass::CallMe", "Hello2!");
            result.ElementAt(2).ShouldBeTraceEnterInto("First.MyClass::CallMe", "param", "Ahoy", "param2", "Ahoy2", "paraNum", "0,5");
            result.ElementAt(3).ShouldBeTraceLeaveFrom("First.MyClass::CallMe", "Ahoy2!");
        }
    }
}
