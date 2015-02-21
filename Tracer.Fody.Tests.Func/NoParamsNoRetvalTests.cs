using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Mono.Cecil;
using NUnit.Framework;
using Tracer.Fody.Tests.Func.MockLoggers;
using Tracer.Fody.Weavers;

namespace Tracer.Fody.Tests.Func
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
