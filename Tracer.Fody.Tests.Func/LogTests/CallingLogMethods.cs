using FluentAssertions;
using NUnit.Framework;
using Tracer.Fody.Tests.Func.MockLoggers;

namespace Tracer.Fody.Tests.Func.LogTests
{
    [TestFixture]
    public class CallingLogMethods : FuncTestBase
    {


        [Test]
        public void Test_Single_LogCall_NoTracing()
        {
            string code = @"
                using System;
                using System.Diagnostics;
                using Tracer.Fody.Tests.Func.MockLoggers;

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
            //result.Count.Should().Be(1);
        }
  
    }
}
