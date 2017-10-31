using System;
using FluentAssertions;
using NUnit.Framework;
using Tracer.Fody.Tests.MockLoggers;

namespace Tracer.Fody.Tests.LogTests
{
    [TestFixture]
    public class CallingLogMethods : TestBase
    {
        [Test]
        public void Test_Logging_Exception()
        {
            string code = @"
                using System;
                using System.Diagnostics;
                using Tracer.Fody.Tests.MockLoggers;

                namespace First
                {
                    public class MyClass
                    {
                        public static void Main()
                        {
                            MockLog.Exception(""Hello"", new ApplicationException(""failed""));
                        }
                    }
                }
            ";

            var result = this.RunTest(code, new PrivateOnlyTraceLoggingFilter(), "First.MyClass::Main");
            result.Count.Should().Be(1);
            result.ElementAt(0).ShouldBeLogCall("First.MyClass::Main", "MockLogException");
        }

        [Test]
        public void Test_Single_LogCall_NoParameter_NoTracing()
        {
            string code = @"
                using System;
                using System.Diagnostics;
                using Tracer.Fody.Tests.MockLoggers;

                namespace First
                {
                    public class MyClass
                    {
                        public static void Main()
                        {
                            MockLog.OuterNoParam();
                        }
                    }
                }
            ";

            var result = this.RunTest(code, new PrivateOnlyTraceLoggingFilter(), "First.MyClass::Main");
            result.Count.Should().Be(1);
            result.ElementAt(0).ShouldBeLogCall("First.MyClass::Main", "MockLogOuterNoParam");
        }

        [Test]
        public void Test_Single_LogCall_NoTracing()
        {
            string code = @"
                using System;
                using System.Diagnostics;
                using Tracer.Fody.Tests.MockLoggers;

                namespace First
                {
                    public class MyClass
                    {
                        public static void Main()
                        {
                            MockLog.Outer(""Hello"");
                        }
                    }
                }
            ";

            var result = this.RunTest(code, new PrivateOnlyTraceLoggingFilter(), "First.MyClass::Main");
            result.Count.Should().Be(1);
            result.ElementAt(0).ShouldBeLogCall("First.MyClass::Main", "MockLogOuter", "Hello");
        }

        [Test]
        public void Test_Multiple_LogCalls_NoTracing()
        {
            string code = @"
                using System;
                using System.Diagnostics;
                using Tracer.Fody.Tests.MockLoggers;

                namespace First
                {
                    public class MyClass
                    {
                        public static void Main()
                        {
                            MockLog.Outer(""Hello"");
                            int i  = 1;
                            MockLog.Outer(""String"", i);
                        }
                    }
                }
            ";

            var result = this.RunTest(code, new PrivateOnlyTraceLoggingFilter(), "First.MyClass::Main");
            result.Count.Should().Be(2);
            result.ElementAt(0).ShouldBeLogCall("First.MyClass::Main", "MockLogOuter", "Hello");
            result.ElementAt(1).ShouldBeLogCall("First.MyClass::Main", "MockLogOuter", "String", "1");
        }

        [Test]
        public void Test_Multiple_LogCalls_WithTracing()
        {
            string code = @"
                using System;
                using System.Diagnostics;
                using Tracer.Fody.Tests.MockLoggers;

                namespace First
                {
                    public class MyClass
                    {
                        public static void Main()
                        {
                            MockLog.Outer(""Hello"");
                            int i  = 1;
                            MockLog.Outer(""String"", i);
                        }
                    }
                }
            ";

            var result = this.RunTest(code, new AllTraceLoggingFilter(), "First.MyClass::Main");
            result.Count.Should().Be(4);
            result.ElementAt(0).ShouldBeTraceEnterInto("First.MyClass::Main");
            result.ElementAt(1).ShouldBeLogCall("First.MyClass::Main", "MockLogOuter", "Hello");
            result.ElementAt(2).ShouldBeLogCall("First.MyClass::Main", "MockLogOuter", "String", "1");
            result.ElementAt(3).ShouldBeTraceLeaveFrom("First.MyClass::Main");
        }

        [Test]
        public void TestLoging_In_Constructor()
        {
            string code = @"
                using System;
                using System.Diagnostics;
                using Tracer.Fody.Tests.MockLoggers;

                namespace First
                {
                    public class MyClass
                    {
                        public MyClass()
                        {
                            MockLog.OuterNoParam();
                        }

                        public static void Main()
                        {
                            var myClass = new MyClass();
                        }
                    }
                }
            ";

            var result = this.RunTest(code, new PrivateOnlyTraceLoggingFilter(), "First.MyClass::Main");
            result.Count.Should().Be(1);
            result.ElementAt(0).ShouldBeLogCall("First.MyClass::.ctor()", "MockLogOuterNoParam");
        }

        [Test]
        public void TestLoging_In_StaticConstructor()
        {
            string code = @"
                using System;
                using System.Diagnostics;
                using Tracer.Fody.Tests.MockLoggers;

                namespace First
                {
                    public class MyClass
                    {
                        static MyClass()
                        {
                            MockLog.OuterNoParam();
                        }

                        public static void Main()
                        {
                            var myClass = new MyClass();
                        }
                    }
                }
            ";

            var result = this.RunTest(code, new PrivateOnlyTraceLoggingFilter(), "First.MyClass::Main");
            result.Count.Should().Be(1);
            result.ElementAt(0).ShouldBeLogCall("First.MyClass::.cctor()", "MockLogOuterNoParam");
        }

        [Test]
        public void TestLoging_In_Lambda()
        {
            string code = @"
                using System;
                using System.Diagnostics;
                using Tracer.Fody.Tests.MockLoggers;

                namespace First
                {
                    public class MyClass
                    {
                        public static void Main()
                        {
                            Action act = () => {
                                MockLog.OuterNoParam();
                            };
                            act();
                        }
                    }
                }
            ";

            var result = this.RunTest(code, new NoTraceLoggingFilter(), "First.MyClass::Main");
            result.Count.Should().Be(1);
            result.ElementAt(0).ShouldBeLogCall("First.MyClass::Main", "MockLogOuterNoParam");
            result.ElementAt(0).ContainingMethod.Should().Be("Main()");
        }

        [Test]
        public void TestLoging_In_Closure()
        {
            string code = @"
                using System;
                using System.Diagnostics;
                using Tracer.Fody.Tests.MockLoggers;

                namespace First
                {
                    public class MyClass
                    {
                        delegate void TestDelegate();

                        public static void Main()
                        {
                            int foo = 1;
                            TestDelegate dlg = () => {
                                foo++;
                                MockLog.OuterNoParam();
                            };
                            dlg();
                        }
                    }
                }
            ";

            var result = this.RunTest(code, new NoTraceLoggingFilter(), "First.MyClass::Main");
            result.Count.Should().Be(1);
            result.ElementAt(0).ShouldBeLogCall("First.MyClass::Main", "MockLogOuterNoParam");
            result.ElementAt(0).ContainingMethod.Should().Be("Main()");
        }

        [Test]
        public void TestStaticPropertyRewrite()
        {
            string code = @"
                using System;
                using System.Diagnostics;
                using Tracer.Fody.Tests.MockLoggers;

                namespace First
                {
                    public class MyClass
                    {
                        public static void Main()
                        {
                            var x = MockLog.IsEnabled;
                        }
                    }
                }
            ";

            var result = this.RunTest(code, new PrivateOnlyTraceLoggingFilter(), "First.MyClass::Main");
            result.Count.Should().Be(1);
            result.ElementAt(0).ShouldBeLogProperty("First.MyClass", "IsEnabled");
        }

        [Test]
        public void StaticPropertyWriteRewriteShouldFail()
        {
            string code = @"
                using System;
                using System.Diagnostics;
                using Tracer.Fody.Tests.MockLoggers;

                namespace First       
                {
                    public class MyClass
                    {
                        public static void Main()
                        {
                            MockLog.ReadWrite = false;
                        }
                    }
                }
            ";

            Action test = () => this.RunTest(code, new PrivateOnlyTraceLoggingFilter(), "First.MyClass::Main");
            test.ShouldThrow<Exception>().And.Message.Contains("not supported");
        }

        [Test]
        public void Test_GenericLogCall()
        {
            string code = @"
                using System;
                using System.Diagnostics;
                using Tracer.Fody.Tests.MockLoggers;

                namespace First
                {
                    public class MyClass
                    {
                        public static void Main()
                        {
                            MockLog.GenericOuter<string>(""Hello"");
                        }
                    }
                }
            ";

            var result = this.RunTest(code, new PrivateOnlyTraceLoggingFilter(), "First.MyClass::Main");
            result.Count.Should().Be(1);
            result.ElementAt(0).ShouldBeLogCall("First.MyClass::Main", "MockLogGenericOuter", "Hello");
        }

        [Test]
        public void Test_MultiGenericLogCall()
        {
            string code = @"
                using System;
                using System.Diagnostics;
                using Tracer.Fody.Tests.MockLoggers;

                namespace First
                {
                    public class MyClass
                    {
                        public static void Main()
                        {
                            MockLog.GenericOuter<string, int>(4, ""Hello"", 2);
                        }
                    }
                }
            ";

            var result = this.RunTest(code, new PrivateOnlyTraceLoggingFilter(), "First.MyClass::Main");
            result.Count.Should().Be(1);
            result.ElementAt(0).ShouldBeLogCall("First.MyClass::Main", "MockLogGenericOuter", "4", "Hello", "2");
        }

        [Test]
        public void Test_GenericLogCallWithoutParameters()
        {
            string code = @"
                using System;
                using System.Diagnostics;
                using Tracer.Fody.Tests.MockLoggers;

                namespace First
                {
                    public class MyClass
                    {
                        public static void Main()
                        {
                            MockLog.GenericOuter<string>();
                        }
                    }
                }
            ";

            var result = this.RunTest(code, new PrivateOnlyTraceLoggingFilter(), "First.MyClass::Main");
            result.Count.Should().Be(1);
            result.ElementAt(0).ShouldBeLogCall("First.MyClass::Main", "MockLogGenericOuter", "String");
        }
    }
}
