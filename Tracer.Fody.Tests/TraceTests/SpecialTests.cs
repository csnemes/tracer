using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Tracer.Fody.Tests.MockLoggers;

namespace Tracer.Fody.Tests.TraceTests
{
    [TestFixture]
    public class SpecialTests : TestBase
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
            Rewrite(assemblyPath, new AllTraceLoggingFilter());
            Rewrite(assemblyPath, new AllTraceLoggingFilter());

            var result = this.RunCode(assemblyPath, "First.MyClass", "Main");

            result.Count.Should().Be(2);
        }



    }
}
