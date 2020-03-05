using System;
using FluentAssertions;
using NUnit.Framework;
using Tracer.Fody.Tests.MockLoggers;

namespace Tracer.Fody.Tests.TraceTests
{
    [TestFixture]
    public class SkippingParamsTests : TestBase
    {
        [Test]
        public void Test_Static_SingleStringParameter_Empty_Method()
        {
            string code = @"
                using System;
                using System.Diagnostics;
                using TracerAttributes;

                namespace TracerAttributes
                {
                        [AttributeUsage(AttributeTargets.Class |AttributeTargets.Parameter | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
                        public class NoTrace : Attribute
                        {
                        }
                }

                namespace First
                {
                    public class MyClass
                    {
                        public static void Main()
                        {
                            SetString(""Hello"");
                        }

                        private static void SetString([NoTrace] string param)
                        {
                        }
                    }
                }
            ";

            var result = this.RunTest(code, new PrivateOnlyTraceLoggingFilter(), "First.MyClass::Main");
            result.Count.Should().Be(2);
            result.ElementAt(0).ShouldBeTraceEnterInto("First.MyClass::SetString");
            result.ElementAt(1).ShouldBeTraceLeaveFrom("First.MyClass::SetString");
        }

        [Test]
        public void Test_Static_SingleIntParameter_Empty_Method()
        {
            string code = @"
                using System;
                using System.Diagnostics;
                using TracerAttributes;

                namespace TracerAttributes
                {
                        [AttributeUsage(AttributeTargets.Class |AttributeTargets.Parameter | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
                        public class NoTrace : Attribute
                        {
                        }
                }

                namespace First
                {
                    public class MyClass
                    {
                        public static void Main()
                        {
                            SetInt(42);
                        }

                        private static void SetInt([NoTrace] int param)
                        {
                        }
                    }
                }
            ";

            var result = this.RunTest(code, new PrivateOnlyTraceLoggingFilter(), "First.MyClass::Main");
            result.Count.Should().Be(2);
            result.ElementAt(0).ShouldBeTraceEnterInto("First.MyClass::SetInt");
            result.ElementAt(1).ShouldBeTraceLeaveFrom("First.MyClass::SetInt");
        }

        [Test]
        public void Test_Static_MultipleParameters()
        {
            string code = @"
                using System;
                using System.Diagnostics;
                using TracerAttributes;

                namespace TracerAttributes
                {
                        [AttributeUsage(AttributeTargets.Class |AttributeTargets.Parameter | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
                        public class NoTrace : Attribute
                        {
                        }
                }

                namespace First
                {
                    public class MyClass
                    {
                        public static void Main()
                        {
                            CallMe(""Hello"", ""Hello2"", 42);
                        }

                        private static void CallMe(string param, [NoTrace] string param2, int paraInt)
                        {
                        }
                    }
                }
            ";

            var result = this.RunTest(code, new PrivateOnlyTraceLoggingFilter(), "First.MyClass::Main");
            result.Count.Should().Be(2);
            result.ElementAt(0).ShouldBeTraceEnterInto("First.MyClass::CallMe", "param", "Hello", "paraInt", "42");
            result.ElementAt(1).ShouldBeTraceLeaveFrom("First.MyClass::CallMe");
        }

        [Test]
        public void Test_Instance_MultipleParameters()
        {
            string code = @"
                using System;
                using System.Diagnostics;
                using TracerAttributes;

                namespace TracerAttributes
                {
                        [AttributeUsage(AttributeTargets.Class |AttributeTargets.Parameter | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
                        public class NoTrace : Attribute
                        {
                        }
                }

                namespace First
                {
                    public class MyClass
                    {
                        public static void Main()
                        {
                            var mc = new MyClass();
                            mc.CallMe(""Hello"", ""Hello2"", 42);
                        }

                        private void CallMe([NoTrace] string param, string param2, [NoTrace] int paraInt)
                        {
                        }
                    }
                }
            ";

            var result = this.RunTest(code, new PrivateOnlyTraceLoggingFilter(), "First.MyClass::Main");
            result.Count.Should().Be(2);
            result.ElementAt(0).ShouldBeTraceEnterInto("First.MyClass::CallMe", "param2", "Hello2");
            result.ElementAt(1).ShouldBeTraceLeaveFrom("First.MyClass::CallMe");
        }

        [Test]
        public void Test_Static_Multiple_Outs_And_ReturnValue()
        {
            string code = @"
                using System;
                using System.Diagnostics;
                using TracerAttributes;

                namespace TracerAttributes
                {
                        [AttributeUsage(AttributeTargets.Class |AttributeTargets.Parameter | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
                        public class NoTrace : Attribute
                        {
                        }
                }

                namespace First
                {
                    public class MyClass
                    {
                        public static void Main()
                        {
                            string inp;
                            int inp2;
                            CallMe(out inp, out inp2);
                        }

                        private static string CallMe([NoTrace] out string param, out int para2)
                        {
                            param = ""rv"";
                            para2 = 42;
                            return ""response"";
                        }
                    }
                }
            ";

            var result = this.RunTest(code, new PrivateOnlyTraceLoggingFilter(), "First.MyClass::Main");
            result.Count.Should().Be(2);
            result.ElementAt(0).ShouldBeTraceEnterInto("First.MyClass::CallMe");
            result.ElementAt(1).ShouldBeTraceLeaveFrom("First.MyClass::CallMe", "response");
            result.ElementAt(1).ShouldBeTraceLeaveWithOutsFrom("First.MyClass::CallMe", "para2", "42");
        }

        [Test]
        public void Test_Static_Empty_Method_Returns_Int()
        {
            string code = @"
                using System;
                using System.Diagnostics;
                using TracerAttributes;

                namespace TracerAttributes
                {
                        [AttributeUsage(AttributeTargets.Class |AttributeTargets.Parameter | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
                        public class NoTrace : Attribute
                        {
                        }

                        [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
                        public class NoReturnTrace : Attribute
                        {
                        }
                }

                namespace First
                {
                    public class MyClass
                    {
                        public static void Main()
                        {
                            var i = GetIntValue();
                        }

                        [NoReturnTrace]
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
            result.ElementAt(1).ShouldBeTraceLeaveFrom("First.MyClass::GetIntValue");
        }

        [Test]
        public void Test_Static_Multiple_Outs_And_SkipReturnValue()
        {
            string code = @"
                using System;
                using System.Diagnostics;
                using TracerAttributes;

                namespace TracerAttributes
                {
                        [AttributeUsage(AttributeTargets.Class |AttributeTargets.Parameter | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
                        public class NoTrace : Attribute
                        {
                        }

                        [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
                        public class NoReturnTrace : Attribute
                        {
                        }
                }

                namespace First
                {
                    public class MyClass
                    {
                        public static void Main()
                        {
                            string inp;
                            int inp2;
                            CallMe(out inp, out inp2);
                        }

                        [NoReturnTrace]
                        private static string CallMe([NoTrace] out string param, out int para2)
                        {
                            param = ""rv"";
                            para2 = 42;
                            return ""response"";
                        }
                    }
                }
            ";

            var result = this.RunTest(code, new PrivateOnlyTraceLoggingFilter(), "First.MyClass::Main");
            result.Count.Should().Be(2);
            result.ElementAt(0).ShouldBeTraceEnterInto("First.MyClass::CallMe");
            result.ElementAt(1).ShouldBeTraceLeaveFrom("First.MyClass::CallMe");
            result.ElementAt(1).ShouldBeTraceLeaveWithOutsFrom("First.MyClass::CallMe", "para2", "42");
        }

        [Test]
        public void Test_AsyncLoggingCallOrderWithIntParameterSomeNoTrace()
        {
            string code = @"
                using System;
                using System.Diagnostics;
                using System.Threading.Tasks;
                using Tracer.Fody.Tests.MockLoggers;
                using TracerAttributes;

                namespace TracerAttributes
                {
                        [AttributeUsage(AttributeTargets.Class |AttributeTargets.Parameter | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
                        public class NoTrace : Attribute
                        {
                        }

                        [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
                        public class NoReturnTrace : Attribute
                        {
                        }
                }

                namespace First
                {
                    public class MyClass
                    {
                        public static void Main()
                        {
                            var myClass = new MyClass();
                            var x1 = myClass.CallMe(21, ""Hello2"", 42).Result;
                            var x2 = myClass.CallMe(22, ""Hello3"", 42).Result;
                        }

                        private async Task<int> CallMe(int param, [NoTrace] string param2, int paraInt)
                        {
                            var result = await Double(paraInt);
                            return result;
                        }

                        private async Task<int> Double(int p)
                        {
                            return await Task.Run(()=>  p * 2);
                        }
                    }
                }
            ";

            var result = this.RunTest(code, new PrivateOnlyTraceLoggingFilter(), "First.MyClass::Main");
            result.Count.Should().Be(8);
            result.ElementAt(0).ShouldBeTraceEnterInto("First.MyClass::CallMe", "param", "21", "paraInt", "42");
            result.ElementAt(1).ShouldBeTraceEnterInto("First.MyClass::Double", "p", "42");
            result.ElementAt(2).ShouldBeTraceLeaveFrom("First.MyClass::Double", "84");
            result.ElementAt(3).ShouldBeTraceLeaveFrom("First.MyClass::CallMe", "84");
            result.ElementAt(4).ShouldBeTraceEnterInto("First.MyClass::CallMe", "param", "22", "paraInt", "42");
            result.ElementAt(5).ShouldBeTraceEnterInto("First.MyClass::Double", "p", "42");
            result.ElementAt(6).ShouldBeTraceLeaveFrom("First.MyClass::Double", "84");
            result.ElementAt(7).ShouldBeTraceLeaveFrom("First.MyClass::CallMe", "84");
        }

        [Test]
        public void Test_AsyncLoggingCallOrderWithIntParameterReturnNoTrace()
        {
            string code = @"
                using System;
                using System.Diagnostics;
                using System.Threading.Tasks;
                using Tracer.Fody.Tests.MockLoggers;
                using TracerAttributes;

                namespace TracerAttributes
                {
                        [AttributeUsage(AttributeTargets.Class |AttributeTargets.Parameter | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
                        public class NoTrace : Attribute
                        {
                        }

                        [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
                        public class NoReturnTrace : Attribute
                        {
                        }
                }

                namespace First
                {
                    public class MyClass
                    {
                        public static void Main()
                        {
                            var myClass = new MyClass();
                            var x1 = myClass.CallMe(21, ""Hello2"", 42).Result;
                            var x2 = myClass.CallMe(22, ""Hello3"", 42).Result;
                        }

                        [NoReturnTrace]
                        private async Task<int> CallMe(int param, [NoTrace] string param2, int paraInt)
                        {
                            var result = await Double(paraInt);
                            return result;
                        }

                        private async Task<int> Double(int p)
                        {
                            return await Task.Run(()=>  p * 2);
                        }
                    }
                }
            ";

            var result = this.RunTest(code, new PrivateOnlyTraceLoggingFilter(), "First.MyClass::Main");
            result.Count.Should().Be(8);
            result.ElementAt(0).ShouldBeTraceEnterInto("First.MyClass::CallMe", "param", "21", "paraInt", "42");
            result.ElementAt(1).ShouldBeTraceEnterInto("First.MyClass::Double", "p", "42");
            result.ElementAt(2).ShouldBeTraceLeaveFrom("First.MyClass::Double", "84");
            result.ElementAt(3).ShouldBeTraceLeaveFrom("First.MyClass::CallMe");
            result.ElementAt(4).ShouldBeTraceEnterInto("First.MyClass::CallMe", "param", "22", "paraInt", "42");
            result.ElementAt(5).ShouldBeTraceEnterInto("First.MyClass::Double", "p", "42");
            result.ElementAt(6).ShouldBeTraceLeaveFrom("First.MyClass::Double", "84");
            result.ElementAt(7).ShouldBeTraceLeaveFrom("First.MyClass::CallMe");
        }
    }
}
