using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using Tracer.Fody.Tests.MockLoggers;

namespace Tracer.Fody.Tests.TraceTests
{
    [TestFixture]
    public class ConstructorTests : TestBase
    {
        [Test]
        public void Test_ConstructorLogging_TurnedOff()
        {
            string code = @"
                using System;
                using System.Diagnostics;
                using System.Threading; 

                namespace First
                {
                    public class MyClass
                    {
                        private MyClass()
                        {
                            Thread.Sleep(10);
                        }

                        public static void Main()
                        {
                            var mc = new MyClass();
                        }
                    }
                }
            ";

            var result = this.RunTest(code, new PrivateOnlyTraceLoggingFilter(), "First.MyClass::Main", false);
            result.Count.Should().Be(0);
        }

        [Test]
        public void Test_NoParam_Constructor()
        {
            string code = @"
                using System;
                using System.Diagnostics;
                using System.Threading; 

                namespace First
                {
                    public class MyClass
                    {
                        private MyClass()
                        {
                            Thread.Sleep(10);
                        }

                        public static void Main()
                        {
                            var mc = new MyClass();
                        }
                    }
                }
            ";

            var result = this.RunTest(code, new PrivateOnlyTraceLoggingFilter(), "First.MyClass::Main", true);
            result.Count.Should().Be(2);
            result.ElementAt(0).ShouldBeTraceEnterInto("First.MyClass::.ctor");
            result.ElementAt(1).ShouldBeTraceLeaveFrom("First.MyClass::.ctor");
            result.ElementAt(1).NumberOfTicks.Should().BeGreaterThan(0);
        }

        [Test]
        public void Test_SingleParam_Constructor()
        {
            string code = @"
                using System;
                using System.Diagnostics;
                using System.Threading; 

                namespace First
                {
                    public class MyClass
                    {
                        private MyClass(string inp)
                        {
                            Thread.Sleep(10);
                        }

                        public static void Main()
                        {
                            var mc = new MyClass(""Hello"");
                        }
                    }
                }
            ";

            var result = this.RunTest(code, new PrivateOnlyTraceLoggingFilter(), "First.MyClass::Main", true);
            result.Count.Should().Be(2);
            result.ElementAt(0).ShouldBeTraceEnterInto("First.MyClass::.ctor", "inp", "Hello");
            result.ElementAt(1).ShouldBeTraceLeaveFrom("First.MyClass::.ctor");
            result.ElementAt(1).NumberOfTicks.Should().BeGreaterThan(0);
        }

        [Test]
        public void Test_MultiParam_Constructor()
        {
            string code = @"
                using System;
                using System.Diagnostics;
                using System.Threading; 

                namespace First
                {
                    public class MyClass
                    {
                        private MyClass(string inp, int inp2)
                        {
                            Thread.Sleep(10);
                        }

                        public static void Main()
                        {
                            var mc = new MyClass(""Hello"", 10);
                        }
                    }
                }
            ";

            var result = this.RunTest(code, new PrivateOnlyTraceLoggingFilter(), "First.MyClass::Main", true);
            result.Count.Should().Be(2);
            result.ElementAt(0).ShouldBeTraceEnterInto("First.MyClass::.ctor", "inp", "Hello", "inp2", "10");
            result.ElementAt(1).ShouldBeTraceLeaveFrom("First.MyClass::.ctor");
            result.ElementAt(1).NumberOfTicks.Should().BeGreaterThan(0);
        }

        [Test]
        public void Test_StaticConstructor_MustNotBeTraced()
        {
            string code = @"
                using System;
                using System.Diagnostics;

                namespace First
                {
                    public class MyClass
                    {
                        private static string x = ""abc"";

                        private MyClass(string inp)
                        {
                        }

                        public static void Main()
                        {
                            var mc = new MyClass(""Hello"");
                        }
                    }
                }
            ";

            var result = this.RunTest(code, new PrivateOnlyTraceLoggingFilter(), "First.MyClass::Main", true);
            result.Count.Should().Be(2);
            result.ElementAt(0).ShouldBeTraceEnterInto("First.MyClass::.ctor", "inp", "Hello");
            result.ElementAt(1).ShouldBeTraceLeaveFrom("First.MyClass::.ctor");
        }
    }
}
