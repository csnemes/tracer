using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Tracer.Fody.Filters;
using Tracer.Fody.Filters.DefaultFilter;
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
                        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method| AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
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
                        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
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

        [Test]
        public void NoTrace_Attribute_On_Property_Skips_Getter()
        {
            string code = @"
                using System;
                using System.Diagnostics;

                namespace TracerAttributes
                {
                        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
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
                            var mc = new MyClass(); 
                            var i = mc.IntValue;
                        }
                        
                        [NoTrace]
                        private int IntValue
                        {
                            get { return 1; }
                        }
                    }
                }
            ";

            var def = new AssemblyLevelTraceOnDefinition(NamespaceScope.All, TraceTargetVisibility.All,
                TraceTargetVisibility.All);
            var result = this.RunTest(code, new DefaultFilter(new[] { def }), "First.MyClass::Main");
            result.Count.Should().Be(0);
        }

        [Test]
        public void NoTrace_Attribute_On_Property_Skips_Setter()
        {
            string code = @"
                using System;
                using System.Diagnostics;

                namespace TracerAttributes
                {
                        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
                        public class NoTrace : Attribute
                        {
                        }
                }

                namespace First
                {
                    using TracerAttributes;

                    public class MyClass
                    {
                        private int _intValue;

                        [NoTrace]
                        public static void Main()
                        {
                            var mc = new MyClass(); 
                            mc.IntValue = 1;
                        }
                        
                        [NoTrace]
                        private int IntValue
                        {
                            set { _intValue = value; }
                        }
                    }
                }
            ";

            var def = new AssemblyLevelTraceOnDefinition(NamespaceScope.All, TraceTargetVisibility.All,
                TraceTargetVisibility.All);
            var result = this.RunTest(code, new DefaultFilter(new[] { def }), "First.MyClass::Main");
            result.Count.Should().Be(0);
        }

        [Test]
        public void TraceOn_Attribute_On_Property_Adds_Getter_And_Setter()
        {
            string code = @"
                using System;
                using System.Diagnostics;

                namespace TracerAttributes
                {
                    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
                    public class TraceOn : Attribute
                    {
                        public TraceTarget Target { get; set; }

                        public TraceOn()
                        {}

                        public TraceOn(TraceTarget traceTarget)
                        {
                            Target = traceTarget;
                        }
                    }
                }

                namespace First
                {
                    using TracerAttributes;

                    public class MyClass
                    {
                        private int _intValue;

                        public static void Main()
                        {
                            var mc = new MyClass(); 
                            var i = mc.IntValue;
                            mc.IntValue = 2;
                        }
                        
                        [TraceOn]
                        private int IntValue
                        {
                            get { return 1; }
                            set { _intValue = value; }
                        }
                    }
                }
            ";

            var def = new AssemblyLevelTraceOnDefinition(NamespaceScope.All, TraceTargetVisibility.Public,
                TraceTargetVisibility.Public);
            var result = this.RunTest(code, new DefaultFilter(new[] { def }), "First.MyClass::Main");
            result.Count.Should().Be(6);
        }
    }
}
