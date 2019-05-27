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

        [Test]
        public void Test_LocalFunction()
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
                            var ext = ""zzz"";
                            CallMe(""Hello"");
                            CallMe(""Ahoy"");

                            string CallMe(string param)
                            {
                                return param + ext;
                            }
                        }
                    }
                }
            ";

            var result = this.RunTest(code, new AllTraceLoggingFilter(), "First.MyClass::Main");
            result.Count.Should().Be(6);
            result.ElementAt(0).ShouldBeTraceEnterInto("First.MyClass::Main");
            result.ElementAt(1).ShouldBeTraceEnterInto("First.MyClass::Main", "param", "Hello");
            result.ElementAt(2).ShouldBeTraceLeaveFrom("First.MyClass::Main", "Hellozzz");
            result.ElementAt(3).ShouldBeTraceEnterInto("First.MyClass::Main", "param", "Ahoy");
            result.ElementAt(4).ShouldBeTraceLeaveFrom("First.MyClass::Main", "Ahoyzzz");
            result.ElementAt(5).ShouldBeTraceLeaveFrom("First.MyClass::Main");
        }

        [Test]
        public void Test_AsyncLocalFunction()
        {
            string code = @"
                using System;
                using System.Diagnostics;
                using System.Threading.Tasks;

                namespace First
                {
                    public class MyClass
                    {
                        public static void Main()
                        {
                            var myClass = new MyClass();
                            var x1 = myClass.CallMeInner(""Hello"", ""Hello2"").Result;
                        }

                        private async Task<string> CallMeInner(string param, string param2)
                        {
                            var ext = ""zzz"";
                            return await CallMe(""Hello"");

                            async Task<string> CallMe(string param3)
                            {
                                return await AddStrings(param3, ext);
                            }
                        }

                        private Task<string> AddStrings(string p1, string p2)
                        {
                            return Task.FromResult(p1 + p2);
                        }
                    }
                }
            ";

            var result = this.RunTest(code, new AllTraceLoggingFilter(), "First.MyClass::Main");
            result.Count.Should().Be(6);
            result.ElementAt(0).ShouldBeTraceEnterInto("First.MyClass::Main");
            result.ElementAt(1).ShouldBeTraceEnterInto("First.MyClass::CallMe", "param", "Hello");
            result.ElementAt(2).ShouldBeTraceEnterInto("First.MyClass::AddStrings", "p1", "Hello", "p2", "zzz");
            result.ElementAt(3).ShouldBeTraceLeaveFrom("First.MyClass::AddStrings", "System.Threading.Tasks.Task`1[System.String]");
            result.ElementAt(4).ShouldBeTraceLeaveFrom("First.MyClass::CallMe","Hellozzz");
            result.ElementAt(5).ShouldBeTraceLeaveFrom("First.MyClass::Main");
        }
    }
}
