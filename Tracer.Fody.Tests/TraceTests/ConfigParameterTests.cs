using System.Collections.Generic;
using FluentAssertions;
using Mono.Cecil;
using NUnit.Framework;
using Tracer.Fody.Filters;
using Tracer.Fody.Helpers;
using Tracer.Fody.Tests.MockLoggers;
using Tracer.Fody.Weavers;

namespace Tracer.Fody.Tests.TraceTests
{
    [TestFixture]
    public class ConfigParameterTests : TestBase
    {
        [Test]
        public void Test_Static_Empty_Method_WithSingleConfigParameter()
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

            //faking config params with filter
            var result = this.RunTest(code, new FakeParamFilter(), "First.MyClass::Main");
            result.Count.Should().Be(2);
            result.ElementAt(0).ShouldBeTraceEnterInto("First.MyClass::Main");
            result.ElementAt(0).ConfigParameters.Length.Should().Be(1);
            result.ElementAt(0).ConfigParameters[0].Item1.Should().Be("IncludeArguments");
            result.ElementAt(0).ConfigParameters[0].Item2.Should().Be("True");
            result.ElementAt(1).ShouldBeTraceLeaveFrom("First.MyClass::Main");
            result.ElementAt(1).ConfigParameters.Length.Should().Be(1);
            result.ElementAt(1).ConfigParameters[0].Item1.Should().Be("IncludeArguments");
            result.ElementAt(1).ConfigParameters[0].Item2.Should().Be("True");
        }

        [Test]
        public void Test_Static_Empty_Method_WithConfigParameters()
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

            //faking config params with filter
            var result = this.RunTest(code, new FakeParamFilterMultiple(), "First.MyClass::Main");
            result.Count.Should().Be(2);
            result.ElementAt(0).ShouldBeTraceEnterInto("First.MyClass::Main");
            result.ElementAt(0).ConfigParameters.Length.Should().Be(2);
            result.ElementAt(0).ConfigParameters[0].Item1.Should().Be("IncludeArguments");
            result.ElementAt(0).ConfigParameters[0].Item2.Should().Be("True");
            result.ElementAt(0).ConfigParameters[1].Item1.Should().Be("Other");
            result.ElementAt(0).ConfigParameters[1].Item2.Should().Be("42");
            result.ElementAt(1).ShouldBeTraceLeaveFrom("First.MyClass::Main");
            result.ElementAt(1).ConfigParameters.Length.Should().Be(2);
            result.ElementAt(1).ConfigParameters[0].Item1.Should().Be("IncludeArguments");
            result.ElementAt(1).ConfigParameters[0].Item2.Should().Be("True");
            result.ElementAt(1).ConfigParameters[1].Item1.Should().Be("Other");
            result.ElementAt(1).ConfigParameters[1].Item2.Should().Be("42");
        }


        [Test]
        public void Test_AsyncNoReturnValue()
        {
            string code = @"
                using System;
                using System.Diagnostics;
                using System.Threading.Tasks;
                using Tracer.Fody.Tests.MockLoggers;

                namespace First
                {
                    public class MyClass
                    {
                        public static void Main()
                        {
                            var myClass = new MyClass();
                            myClass.CallMe(""Hello"", ""Hello2"", 42).Wait();
                        }

                        private async Task CallMe(string param, string param2, int paraInt)
                        {
                            var result = await Double(paraInt);
                            return;
                        }

                        private async Task<int> Double(int p)
                        {
                            return await Task.Run(()=>  p * 2);
                        }
                    }
                }
            ";

            //faking config params with filter
            var result = this.RunTest(code, new FakeParamFilterMultiple(), "First.MyClass::Main");
            result.Count.Should().Be(2);
            result.ElementAt(0).ShouldBeTraceEnterInto("First.MyClass::CallMe", "param", "Hello", "param2", "Hello2", "paraInt", "42");
            result.ElementAt(0).ConfigParameters.Length.Should().Be(2);
            result.ElementAt(0).ConfigParameters[0].Item1.Should().Be("IncludeArguments");
            result.ElementAt(0).ConfigParameters[0].Item2.Should().Be("True");
            result.ElementAt(0).ConfigParameters[1].Item1.Should().Be("Other");
            result.ElementAt(0).ConfigParameters[1].Item2.Should().Be("42");
            result.ElementAt(3).ShouldBeTraceLeaveFrom("First.MyClass::CallMe");
            result.ElementAt(3).ConfigParameters.Length.Should().Be(2);
            result.ElementAt(3).ConfigParameters[0].Item1.Should().Be("IncludeArguments");
            result.ElementAt(3).ConfigParameters[0].Item2.Should().Be("True");
            result.ElementAt(3).ConfigParameters[1].Item1.Should().Be("Other");
            result.ElementAt(3).ConfigParameters[1].Item2.Should().Be("42");
        }

        private class FakeParamFilter : ITraceLoggingFilter
        {
            public FilterResult ShouldAddTrace(MethodDefinition definition) => new FilterResult(true, new Dictionary<string, string>
            {
                ["IncludeArguments"] = "True",
            });

            public void LogFilterInfo(IWeavingLogger weavingLogger)
            {
            }

        }

        private class FakeParamFilterMultiple : ITraceLoggingFilter
        {
            public FilterResult ShouldAddTrace(MethodDefinition definition) => new FilterResult(true, new Dictionary<string, string>
            {
                ["IncludeArguments"] = "True",
                ["Other"] = "42"
            });

            public void LogFilterInfo(IWeavingLogger weavingLogger)
            {
            }

        }
    }
}
