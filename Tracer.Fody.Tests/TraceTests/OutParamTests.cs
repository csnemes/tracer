using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Tracer.Fody.Tests.MockLoggers;

namespace Tracer.Fody.Tests.TraceTests
{
    [TestFixture]
    public class OutParamTests : TestBase
    {
        [Test]
        public void Test_Static_And_ReturnValue()
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
                            string inp;
                            CallMe(out inp);
                        }

                        private static string CallMe(out string param)
                        {
                            param = ""rv"";
                            return ""response"";
                        }
                    }
                }
            ";

            var result = this.RunTest(code, new PrivateOnlyTraceLoggingFilter(), "First.MyClass::Main");
            result.Count.Should().Be(2);
            result.ElementAt(0).ShouldBeTraceEnterInto("First.MyClass::CallMe");
            result.ElementAt(1).ShouldBeTraceLeaveFrom("First.MyClass::CallMe", "response");
            result.ElementAt(1).ShouldBeTraceLeaveWithOutsFrom("First.MyClass::CallMe", "param", "rv");
        }

        [Test]
        public void Test_Static_Multiple_Outs_And_ReturnValue()
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
                            string inp;
                            int inp2;
                            CallMe(out inp, out inp2);
                        }

                        private static string CallMe(out string param, out int para2)
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
            result.ElementAt(1).ShouldBeTraceLeaveWithOutsFrom("First.MyClass::CallMe", "param", "rv", "para2", "42");
        }

        [Test]
        public void Test_Static_String()
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
                            string inp;
                            CallMe(out inp);
                        }

                        private static void CallMe(out string param)
                        {
                            param = ""rv"";
                        }
                    }
                }
            ";

            var result = this.RunTest(code, new PrivateOnlyTraceLoggingFilter(), "First.MyClass::Main");
            result.Count.Should().Be(2);
            result.ElementAt(0).ShouldBeTraceEnterInto("First.MyClass::CallMe");
            result.ElementAt(1).ShouldBeTraceLeaveWithOutsFrom("First.MyClass::CallMe", "param", "rv");
        }

        [Test]
        public void Test_Static_Int()
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
                            int inp;
                            CallMe(out inp);
                        }

                        private static void CallMe(out int param)
                        {
                            param = 42;
                        }
                    }
                }
            ";

            var result = this.RunTest(code, new PrivateOnlyTraceLoggingFilter(), "First.MyClass::Main");
            result.Count.Should().Be(2);
            result.ElementAt(0).ShouldBeTraceEnterInto("First.MyClass::CallMe");
            result.ElementAt(1).ShouldBeTraceLeaveWithOutsFrom("First.MyClass::CallMe", "param", "42");
        }

        [Test]
        public void Test_Static_Long()
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
                            long inp;
                            CallMe(out inp);
                        }

                        private static void CallMe(out long param)
                        {
                            param = 42;
                        }
                    }
                }
            ";

            var result = this.RunTest(code, new PrivateOnlyTraceLoggingFilter(), "First.MyClass::Main");
            result.Count.Should().Be(2);
            result.ElementAt(0).ShouldBeTraceEnterInto("First.MyClass::CallMe");
            result.ElementAt(1).ShouldBeTraceLeaveWithOutsFrom("First.MyClass::CallMe", "param", "42");
        }

        [Test]
        public void Test_Static_Float()
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
                            float inp;
                            CallMe(out inp);
                        }

                        private static void CallMe(out float param)
                        {
                            param = 42.5F;
                        }
                    }
                }
            ";

            var result = this.RunTest(code, new PrivateOnlyTraceLoggingFilter(), "First.MyClass::Main");
            result.Count.Should().Be(2);
            result.ElementAt(0).ShouldBeTraceEnterInto("First.MyClass::CallMe");
            result.ElementAt(1).ShouldBeTraceLeaveWithOutsFrom("First.MyClass::CallMe", "param", 42.5F.ToString());
        }

        [Test]
        public void Test_Static_Double()
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
                            double inp;
                            CallMe(out inp);
                        }

                        private static void CallMe(out double param)
                        {
                            param = 42.5;
                        }
                    }
                }
            ";

            var result = this.RunTest(code, new PrivateOnlyTraceLoggingFilter(), "First.MyClass::Main");
            result.Count.Should().Be(2);
            result.ElementAt(0).ShouldBeTraceEnterInto("First.MyClass::CallMe");
            result.ElementAt(1).ShouldBeTraceLeaveWithOutsFrom("First.MyClass::CallMe", "param", 42.5.ToString());
        }

        [Test]
        public void Test_Static_UnsignedInt()
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
                            uint inp;
                            CallMe(out inp);
                        }

                        private static void CallMe(out uint param)
                        {
                            param = 42;
                        }
                    }
                }
            ";

            var result = this.RunTest(code, new PrivateOnlyTraceLoggingFilter(), "First.MyClass::Main");
            result.Count.Should().Be(2);
            result.ElementAt(0).ShouldBeTraceEnterInto("First.MyClass::CallMe");
            result.ElementAt(1).ShouldBeTraceLeaveWithOutsFrom("First.MyClass::CallMe", "param", "42");
        }

        [Test]
        public void Test_Static_UnsignedLong()
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
                            ulong inp;
                            CallMe(out inp);
                        }

                        private static void CallMe(out ulong param)
                        {
                            param = 42;
                        }
                    }
                }
            ";

            var result = this.RunTest(code, new PrivateOnlyTraceLoggingFilter(), "First.MyClass::Main");
            result.Count.Should().Be(2);
            result.ElementAt(0).ShouldBeTraceEnterInto("First.MyClass::CallMe");
            result.ElementAt(1).ShouldBeTraceLeaveWithOutsFrom("First.MyClass::CallMe", "param", "42");
        }

        [Test]
        public void Test_Static_SignedByte()
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
                            sbyte inp;
                            CallMe(out inp);
                        }

                        private static void CallMe(out sbyte param)
                        {
                            param = -128;
                        }
                    }
                }
            ";

            var result = this.RunTest(code, new PrivateOnlyTraceLoggingFilter(), "First.MyClass::Main");
            result.Count.Should().Be(2);
            result.ElementAt(0).ShouldBeTraceEnterInto("First.MyClass::CallMe");
            result.ElementAt(1).ShouldBeTraceLeaveWithOutsFrom("First.MyClass::CallMe", "param", "-128");
        }

        [Test]
        public void Test_Static_Byte()
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
                            byte inp;
                            CallMe(out inp);
                        }

                        private static void CallMe(out byte param)
                        {
                            param = 255;
                        }
                    }
                }
            ";

            var result = this.RunTest(code, new PrivateOnlyTraceLoggingFilter(), "First.MyClass::Main");
            result.Count.Should().Be(2);
            result.ElementAt(0).ShouldBeTraceEnterInto("First.MyClass::CallMe");
            result.ElementAt(1).ShouldBeTraceLeaveWithOutsFrom("First.MyClass::CallMe", "param", "255");
        }

        [Test]
        public void Test_Static_Short()
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
                            short inp;
                            CallMe(out inp);
                        }

                        private static void CallMe(out short param)
                        {
                            param = -42;
                        }
                    }
                }
            ";

            var result = this.RunTest(code, new PrivateOnlyTraceLoggingFilter(), "First.MyClass::Main");
            result.Count.Should().Be(2);
            result.ElementAt(0).ShouldBeTraceEnterInto("First.MyClass::CallMe");
            result.ElementAt(1).ShouldBeTraceLeaveWithOutsFrom("First.MyClass::CallMe", "param", "-42");
        }

        [Test]
        public void Test_Static_Struct()
        {
            string code = @"
                using System;
                using System.Diagnostics;

                namespace First
                {
                    public struct MyStruct
                    {
                        public string Value { get; set; }
                    }

                    public class MyClass
                    {
                        public static void Main()
                        {
                            MyStruct inp;
                            CallMe(out inp);
                        }

                        private static void CallMe(out MyStruct param)
                        {
                            param = new MyStruct() { Value=""Hi"" };
                        }
                    }
                }
            ";

            var result = this.RunTest(code, new PrivateOnlyTraceLoggingFilter(), "First.MyClass::Main");
            result.Count.Should().Be(2);
            result.ElementAt(0).ShouldBeTraceEnterInto("First.MyClass::CallMe");
            result.ElementAt(1).ShouldBeTraceLeaveWithOutsFrom("First.MyClass::CallMe", "param", "First.MyStruct");
        }

        [Test]
        public void Test_Static_Class()
        {
            string code = @"
                using System;
                using System.Diagnostics;

                namespace First
                {
                    public class InnerClass
                    {
                        public string Value { get; set; }
                    }

                    public class MyClass
                    {
                        public static void Main()
                        {
                            InnerClass inp;
                            CallMe(out inp);
                        }

                        private static void CallMe(out InnerClass param)
                        {
                            param = new InnerClass() { Value=""Hi"" };
                        }
                    }
                }
            ";

            var result = this.RunTest(code, new PrivateOnlyTraceLoggingFilter(), "First.MyClass::Main");
            result.Count.Should().Be(2);
            result.ElementAt(0).ShouldBeTraceEnterInto("First.MyClass::CallMe");
            result.ElementAt(1).ShouldBeTraceLeaveWithOutsFrom("First.MyClass::CallMe", "param", "First.InnerClass"); //ToString=First.InnerClass
        }

        [Test]
        public void Test_Instance_Generic()
        {
            string code = @"
                using System;
                using System.Diagnostics;

                namespace First
                {
                    public class StartupClass
                    {
                        public static void Main()
                        {
                            var mc1 = new MyClass<string>();
                            string outp1;    
                            mc1.CallMe(out outp1);
                            var mc2 = new MyClass<int>();
                            int outp2;    
                            mc2.CallMe(out outp2);
                        }
                    }

                    public class MyClass<T>
                    {
                        internal void CallMe(out T param)
                        {
                            param = default(T);
                        }
                    }
                }
            ";

            var result = this.RunTest(code, new InternalOnlyTraceLoggingFilter(), "First.StartupClass::Main");
            result.Count.Should().Be(4);
            result.ElementAt(0).ShouldBeTraceEnterInto("First.MyClass<String>::CallMe");
            result.ElementAt(1).ShouldBeTraceLeaveWithOutsFrom("First.MyClass<String>::CallMe", "param", null);
            result.ElementAt(2).ShouldBeTraceEnterInto("First.MyClass<Int32>::CallMe");
            result.ElementAt(3).ShouldBeTraceLeaveWithOutsFrom("First.MyClass<Int32>::CallMe", "param", "0");
        }

    }
}
