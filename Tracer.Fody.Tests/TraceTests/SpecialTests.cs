using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Tracer.Fody.Filters;
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


        [Test]
        public void NoTrace_Attribute_On_Class_Skips_Method()
        {
            string code = @"
                using System;
                using System.Diagnostics;
                using System.Threading; 

                namespace TracerAttributes
                {
                        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
                        public class NoTrace : Attribute
                        {
                        }
                }

                namespace First
                {
                    using TracerAttributes;

                    [NoTrace]
                    public class MyClass
                    {
                        public static void Main()
                        {
                            Thread.Sleep(10);
                        }
                    }
                }
            ";

            var def = new AssemblyLevelTraceOnDefinition(NamespaceScope.All,  TraceTargetVisibility.All, 
                TraceTargetVisibility.Public);
            var result = this.RunTest(code, new DefaultFilter(new [] { def }), "First.MyClass::Main");
            result.Count.Should().Be(0);
        }


        [Test]
        public void NoTrace_Attribute_On_Method_Skips_Method()
        {
            string code = @"
                using System;
                using System.Diagnostics;
                using System.Threading; 

                namespace TracerAttributes
                {
                        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
                        public class NoTrace : Attribute
                        {
                        }
                }

                namespace First
                {
                    using TracerAttributes;

                    public class MyClass
                    {
                        [NoTrace]
                        public static void Main()
                        {
                            Thread.Sleep(10);
                        }
                    }
                }
            ";

            var def = new AssemblyLevelTraceOnDefinition(NamespaceScope.All, TraceTargetVisibility.All,
                TraceTargetVisibility.Public);
            var result = this.RunTest(code, new DefaultFilter(new[] { def }), "First.MyClass::Main");
            result.Count.Should().Be(0);
        }

    }
}
