using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Tracer.Fody.Tests.MockLoggers;

namespace Tracer.Fody.Tests.TraceTests
{
    [TestFixture]
    public class ExceptionTests : TestBase
    {
        [Test]
        public void Test_Static_Empty_Method()
        {
            string code = @"
                using System;
                using System.Diagnostics;
                using System.Threading; 

                namespace First
                {
                    public class MyClass
                    {
                        public static void Main()
                        {
                            try { MyStatic(); } catch { }
                        }

                        public static void MyStatic()
                        {
                            throw new ApplicationException(""failed"");
                        }
                    }
                }
            ";

            var result = this.RunTest(code, new AllTraceLoggingFilter(), "First.MyClass::Main");
            result.Count.Should().Be(4);
            result.ElementAt(0).ShouldBeTraceEnterInto("First.MyClass::Main");
            result.ElementAt(1).ShouldBeTraceEnterInto("First.MyClass::MyStatic");
            result.ElementAt(2).ShouldBeTraceLeaveWithExceptionFrom("First.MyClass::MyStatic", "failed");
            result.ElementAt(3).ShouldBeTraceLeaveFrom("First.MyClass::Main");
        }

        [Test]
        public void Test_Instance_Empty_Method()
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
                            try {
                            var myClass = new MyClass();
                            myClass.Run(); }
                            catch { }
                        }

                        public void Run()
                        {
                            throw new ApplicationException(""failed"");
                        }
                    }
                }
            ";

            var result = this.RunTest(code, new AllTraceLoggingFilter(), "First.MyClass::Main");
            result.Count.Should().Be(4);
            result.ElementAt(0).ShouldBeTraceEnterInto("First.MyClass::Main");
            result.ElementAt(1).ShouldBeTraceEnterInto("First.MyClass::Run");
            result.ElementAt(2).ShouldBeTraceLeaveWithExceptionFrom("First.MyClass::Run", "failed");
            result.ElementAt(3).ShouldBeTraceLeaveFrom("First.MyClass::Main");
        }

        [Test]
        public void Test_Nesting_Of_Methods()
        {
            string code = @"
                using System;
                using System.Diagnostics;
                using System.Threading; 

                namespace First
                {
                    public class MyClass
                    {
                        public static void Main()
                        {
                            try { MyStatic(); } catch { }
                        }

                        public static void MyStatic()
                        {
                            MyInnerStatic();
                        }

                        public static void MyInnerStatic()
                        {
                            throw new ApplicationException(""failed"");
                        }
                    }
                }
            ";

            var result = this.RunTest(code, new AllTraceLoggingFilter(), "First.MyClass::Main");
            result.Count.Should().Be(6);
            result.ElementAt(0).ShouldBeTraceEnterInto("First.MyClass::Main");
            result.ElementAt(1).ShouldBeTraceEnterInto("First.MyClass::MyStatic");
            result.ElementAt(2).ShouldBeTraceEnterInto("First.MyClass::MyInnerStatic");
            result.ElementAt(3).ShouldBeTraceLeaveWithExceptionFrom("First.MyClass::MyInnerStatic", "failed");
            result.ElementAt(4).ShouldBeTraceLeaveFrom("First.MyClass::MyStatic");
            result.ElementAt(5).ShouldBeTraceLeaveFrom("First.MyClass::Main");
        }

        [Test]
        public void Test_Different_Execution_Paths()
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
                            try {
                            var myClass = new MyClass();
                            myClass.Run(1);
                            myClass.Run(0); }
                            catch { }
                        }

                        public void Run(int inp)
                        {
                            int resp = 1/inp;
                        }
                    }
                }
            ";

            var result = this.RunTest(code, new AllTraceLoggingFilter(), "First.MyClass::Main");
            result.Count.Should().Be(6);
            result.ElementAt(0).ShouldBeTraceEnterInto("First.MyClass::Main");
            result.ElementAt(1).ShouldBeTraceEnterInto("First.MyClass::Run", "inp", "1");
            result.ElementAt(2).ShouldBeTraceLeaveFrom("First.MyClass::Run");
            result.ElementAt(3).ShouldBeTraceEnterInto("First.MyClass::Run", "inp", "0");
            result.ElementAt(4).ShouldBeTraceLeaveWithExceptionFrom("First.MyClass::Run", "divide by zero");
            result.ElementAt(5).ShouldBeTraceLeaveFrom("First.MyClass::Main");
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
                            try {
                            CallMe(""Hello"", ""Hello2"", 42); } catch {}
                        }

                        private static string CallMe(string param, string param2, int paraInt)
                        {
                            throw new ApplicationException(""failed"");
                            return ""response"" + paraInt.ToString();
                        }
                    }
                }
            ";

            var result = this.RunTest(code, new PrivateOnlyTraceLoggingFilter(), "First.MyClass::Main");
            result.Count.Should().Be(2);
            result.ElementAt(0).ShouldBeTraceEnterInto("First.MyClass::CallMe", "param", "Hello", "param2", "Hello2", "paraInt", "42");
            result.ElementAt(1).ShouldBeTraceLeaveWithExceptionFrom("First.MyClass::CallMe", "failed");
        }
    }
}
