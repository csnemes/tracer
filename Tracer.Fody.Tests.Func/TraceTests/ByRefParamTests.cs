using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Tracer.Fody.Tests.Func.MockLoggers;

namespace Tracer.Fody.Tests.Func.TraceTests
{
    [TestFixture]
    public class ByRefParamTests : FuncTestBase
    {
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
                            string inp = ""goinIn"";
                            CallMe(ref inp);
                        }

                        private static void CallMe(ref string param)
                        {
                            param = ""goinOut"";
                        }
                    }
                }
            ";

            var result = this.RunTest(code, new PrivateOnlyTraceLoggingFilter(), "First.MyClass::Main");
            result.Count.Should().Be(2);
            result.ElementAt(0).ShouldBeTraceEnterInto("First.MyClass::CallMe", "param", "goinIn");
            result.ElementAt(1).ShouldBeTraceLeaveWithOutsFrom("First.MyClass::CallMe", "param", "goinOut");
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
                            int inp = 42;
                            CallMe(ref inp);
                        }

                        private static void CallMe(ref int param)
                        {
                            param = 24;
                        }
                    }
                }
            ";

            var result = this.RunTest(code, new PrivateOnlyTraceLoggingFilter(), "First.MyClass::Main");
            result.Count.Should().Be(2);
            result.ElementAt(0).ShouldBeTraceEnterInto("First.MyClass::CallMe", "param", "42");
            result.ElementAt(1).ShouldBeTraceLeaveWithOutsFrom("First.MyClass::CallMe", "param", "24");
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
                            MyStruct inp = new MyStruct() { IntVal = 42 };
                            CallMe(ref inp);
                        }

                        private static void CallMe(ref MyStruct param)
                        {
                            param = new MyStruct() { IntVal = 24 };
                        }
                    }
                }
            ";

            var result = this.RunTest(code, new PrivateOnlyTraceLoggingFilter(), "First.MyClass::Main");
            result.Count.Should().Be(2);
            result.ElementAt(0).ShouldBeTraceEnterInto("First.MyClass::CallMe", "param", "I42");
            result.ElementAt(1).ShouldBeTraceLeaveWithOutsFrom("First.MyClass::CallMe", "param", "I24");
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
                            InnerClass inp = new InnerClass() { IntVal = 42 };
                            CallMe(ref inp);
                        }

                        private static void CallMe(ref InnerClass param)
                        {
                            param = new InnerClass() { IntVal = 24 };
                        }
                    }
                }
            ";

            var result = this.RunTest(code, new PrivateOnlyTraceLoggingFilter(), "First.MyClass::Main");
            result.Count.Should().Be(2);
            result.ElementAt(0).ShouldBeTraceEnterInto("First.MyClass::CallMe", "param", "I42");
            result.ElementAt(1).ShouldBeTraceLeaveWithOutsFrom("First.MyClass::CallMe", "param", "I24");
        }

    }
}
