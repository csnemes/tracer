using FluentAssertions;
using NUnit.Framework;
using Tracer.Fody.Tests.Func.MockLoggers;

namespace Tracer.Fody.Tests.Func.TraceTests
{
    [TestFixture]
    public class NoParamsNoRetvalTests : FuncTestBase
    {


        [Test]
        public void Test_Static_Empty_Method()
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
                        }
                    }
                }
            ";

            var result = this.RunTest(code, new NullTraceLoggingFilter(), "First.MyClass::Main");
            result.Count.Should().Be(2);
            result.ElementAt(0).ShouldBeTraceEnterInto("First.MyClass::Main");
            result.ElementAt(1).ShouldBeTraceLeaveFrom("First.MyClass::Main");
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
                            var myClass = new MyClass();
                            myClass.Run();
                        }

                        public void Run()
                        {
                        }
                    }
                }
            ";

            var result = this.RunTest(code, new NullTraceLoggingFilter(), "First.MyClass::Main");
            result.Count.Should().Be(4);
            result.ElementAt(0).ShouldBeTraceEnterInto("First.MyClass::Main");
            result.ElementAt(1).ShouldBeTraceEnterInto("First.MyClass::Run");
            result.ElementAt(2).ShouldBeTraceLeaveFrom("First.MyClass::Run");
            result.ElementAt(3).ShouldBeTraceLeaveFrom("First.MyClass::Main");
        }



    }
}
