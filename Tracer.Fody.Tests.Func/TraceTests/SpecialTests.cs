using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Tracer.Fody.Tests.Func.MockLoggers;

namespace Tracer.Fody.Tests.Func.TraceTests
{
    [TestFixture]
    public class SpecialTests : FuncTestBase
    {
        [Test]
        public void Weaving_Twice_Second_Time_No_Weave()
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
                            Thread.Sleep(10);
                        }
                    }
                }
            ";

            var testDllLocation = new Uri(Assembly.GetExecutingAssembly().CodeBase);
            var assemblyPath = Compile(code, "testasm", new[] { testDllLocation.AbsolutePath });
            Rewrite(assemblyPath, new NullTraceLoggingFilter());
            Rewrite(assemblyPath, new NullTraceLoggingFilter());

            var result = this.RunCode(assemblyPath, "First.MyClass", "Main");

            result.Count.Should().Be(2);
        }



    }
}
