using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using Tracer.Fody.Tests.MockLoggers;

namespace Tracer.Fody.Tests.TraceTests
{
    //[TestFixture]
    public class AsyncTests : TestBase
    {
        [Test]
        public void Test_AsyncLoggingCallOrder()
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
                            var x1 = myClass.CallMe(""Hello"", ""Hello2"", 42).Result;
                        }

                        private async Task<int> CallMe(string param, string param2, int paraInt)
                        {
                            var result = await Double(paraInt);
                            return result;
                        }

                        private async Task<int> Double(int p)
                        {
                            return await Task.Run(()=>  p * 2);
                        }
                    }
                }
            ";

            var result = this.RunTest(code, new PrivateOnlyTraceLoggingFilter(), "First.MyClass::Main");
            result.Count.Should().Be(4);
            result.ElementAt(0).ShouldBeTraceEnterInto("First.MyClass::CallMe", "param", "Hello", "param2", "Hello2", "paraInt", "42");
            result.ElementAt(1).ShouldBeTraceEnterInto("First.MyClass::Double", "p", "42");
            result.ElementAt(2).ShouldBeTraceLeaveFrom("First.MyClass::Double", "84");
            result.ElementAt(3).ShouldBeTraceLeaveFrom("First.MyClass::CallMe", "84");
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
                            myClass.CallMe(""Ahoy"", ""Ahoy2"", 43).Wait();
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

            var result = this.RunTest(code, new PrivateOnlyTraceLoggingFilter(), "First.MyClass::Main");
            result.Count.Should().Be(4);
            //result.ElementAt(0).ShouldBeTraceEnterInto("First.MyClass::CallMe", "param", "Hello", "param2", "Hello2", "paraInt", "42");
            //result.ElementAt(1).ShouldBeTraceLeaveFrom("First.MyClass::CallMe", "84");
            //result.ElementAt(2).ShouldBeTraceEnterInto("First.MyClass::CallMe", "param", "Ahoy", "param2", "Ahoy2", "paraInt", "43");
            //result.ElementAt(3).ShouldBeTraceLeaveFrom("First.MyClass::CallMe", "86");
        }

        [Test]
        public void Test_Async()
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
                            var x1 = myClass.CallMe(""Hello"", ""Hello2"", 42).Result;
                            var x2 = myClass.CallMe(""Ahoy"", ""Ahoy2"", 43).Result;
                        }

                        private async Task<int> CallMe(string param, string param2, int paraInt)
                        {
                            var result = await Double(paraInt);
                            MockLog.OuterNoParam();
                            return result;
                        }

                        private async Task<int> Double(int p)
                        {
                            MockLog.OuterNoParam();
                            return await Task.Run(()=>  p * 2);
                        }
                    }
                }
            ";

            var result = this.RunTest(code, new PrivateOnlyTraceLoggingFilter(), "First.MyClass::Main");
            result.Count.Should().Be(4);
            //result.ElementAt(0).ShouldBeTraceEnterInto("First.MyClass::CallMe", "param", "Hello", "param2", "Hello2", "paraInt", "42");
            //result.ElementAt(1).ShouldBeTraceLeaveFrom("First.MyClass::CallMe", "84");
            //result.ElementAt(2).ShouldBeTraceEnterInto("First.MyClass::CallMe", "param", "Ahoy", "param2", "Ahoy2", "paraInt", "43");
            //result.ElementAt(3).ShouldBeTraceLeaveFrom("First.MyClass::CallMe", "86");
        }

        [Test]
        public void Test_AsyncString()
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
                            var x1 = myClass.CallMe(""Hello"", ""Hello2"", 42).Result;
                            var x2 = myClass.CallMe(""Ahoy"", ""Ahoy2"", 43).Result;
                        }

                        private async Task<string> CallMe(string param, string param2, int paraInt)
                        {
                            var result = await Double(paraInt);
                            //MockLog.OuterNoParam();
                            return result;
                        }

                        private async Task<string> Double(int p)
                        {
                            //MockLog.OuterNoParam();
                            return await Task.Run(()=>  p.ToString());
                        }
                    }
                }
            ";

            var result = this.RunTest(code, new PrivateOnlyTraceLoggingFilter(), "First.MyClass::Main");
            result.Count.Should().Be(4);
            //result.ElementAt(0).ShouldBeTraceEnterInto("First.MyClass::CallMe", "param", "Hello", "param2", "Hello2", "paraInt", "42");
            //result.ElementAt(1).ShouldBeTraceLeaveFrom("First.MyClass::CallMe", "84");
            //result.ElementAt(2).ShouldBeTraceEnterInto("First.MyClass::CallMe", "param", "Ahoy", "param2", "Ahoy2", "paraInt", "43");
            //result.ElementAt(3).ShouldBeTraceLeaveFrom("First.MyClass::CallMe", "86");
        }

        [Test]
        public void Test_AsyncNoParam()
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
                            myClass.CallMe(""Ahoy"", ""Ahoy2"", 43).Wait();
                        }

                        private async Task CallMe(string param, string param2, int paraInt)
                        {
                            var result = await Double(paraInt);
                            MockLog.OuterNoParam();
                            return;
                        }

                        private async Task<int> Double(int p)
                        {
                            MockLog.OuterNoParam();
                            return await Task.Run(()=>  p * 2);
                        }
                    }
                }
            ";

            var result = this.RunTest(code, new PrivateOnlyTraceLoggingFilter(), "First.MyClass::Main");
            result.Count.Should().Be(4);
            //result.ElementAt(0).ShouldBeTraceEnterInto("First.MyClass::CallMe", "param", "Hello", "param2", "Hello2", "paraInt", "42");
            //result.ElementAt(1).ShouldBeTraceLeaveFrom("First.MyClass::CallMe", "84");
            //result.ElementAt(2).ShouldBeTraceEnterInto("First.MyClass::CallMe", "param", "Ahoy", "param2", "Ahoy2", "paraInt", "43");
            //result.ElementAt(3).ShouldBeTraceLeaveFrom("First.MyClass::CallMe", "86");
        }

        [Test]
        public void Test_AsyncObj()
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
                            var x1 = myClass.CallMe(""Hello"", ""Hello2"", 42).Result;
                            var x2 = myClass.CallMe(""Ahoy"", ""Ahoy2"", 43).Result;
                        }
                        
                        private OtherClass _otc = new OtherClass();
                        private int _num = 2;

                        private async Task<int> CallMe(string param, string param2, int paraInt)
                        {
                            var result = await _otc.Double(paraInt);
                            MockLog.OuterNoParam();
                            return result * _num;
                        }

                    }

                    public class OtherClass
                    {
                        public async Task<int> Double(int p)
                        {
                            MockLog.OuterNoParam();
                            return await Task.Run(()=>  p * 2);
                        }
                    }
                }
            ";

            var result = this.RunTest(code, new PrivateOnlyTraceLoggingFilter(), "First.MyClass::Main");
            result.Count.Should().Be(4);
            //result.ElementAt(0).ShouldBeTraceEnterInto("First.MyClass::CallMe", "param", "Hello", "param2", "Hello2", "paraInt", "42");
            //result.ElementAt(1).ShouldBeTraceLeaveFrom("First.MyClass::CallMe", "84");
            //result.ElementAt(2).ShouldBeTraceEnterInto("First.MyClass::CallMe", "param", "Ahoy", "param2", "Ahoy2", "paraInt", "43");
            //result.ElementAt(3).ShouldBeTraceLeaveFrom("First.MyClass::CallMe", "86");
        }

        [Test]
        public void Test_GenericAsync()
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
                            var x1 = myClass.CallMe<string>(""Hello"", ""Hello2"", 42).Result;
                            var x2 = myClass.CallMe<int>(12, ""Ahoy2"", 43).Result;
                        }

                        private async Task<int> CallMe<T>(T param, string param2, int paraInt)
                        {
                            var result = await Double(paraInt);
                            MockLog.OuterNoParam();
                            return result;
                        }

                        private async Task<int> Double(int p)
                        {
                            MockLog.OuterNoParam();
                            return await Task.Run(()=>  p * 2);
                        }
                    }
                }
            ";

            var result = this.RunTest(code, new PrivateOnlyTraceLoggingFilter(), "First.MyClass::Main");
            result.Count.Should().Be(4);
            //result.ElementAt(0).ShouldBeTraceEnterInto("First.MyClass::CallMe", "param", "Hello", "param2", "Hello2", "paraInt", "42");
            //result.ElementAt(1).ShouldBeTraceLeaveFrom("First.MyClass::CallMe", "84");
            //result.ElementAt(2).ShouldBeTraceEnterInto("First.MyClass::CallMe", "param", "Ahoy", "param2", "Ahoy2", "paraInt", "43");
            //result.ElementAt(3).ShouldBeTraceLeaveFrom("First.MyClass::CallMe", "86");
        }

        [Test]
        public void Test_GenericAsyncRetval()
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
                            var x1 = myClass.CallMe<string>(""Hello"", ""Hello2"", 42).Result;
                            var x2 = myClass.CallMe<int>(12, ""Ahoy2"", 43).Result;
                        }

                        private async Task<T> CallMe<T>(T param, string param2, int paraInt)
                        {
                            var result = await Double(paraInt);
                            MockLog.OuterNoParam();
                            return param;
                        }

                        private async Task<int> Double(int p)
                        {
                            MockLog.OuterNoParam();
                            return await Task.Run(()=>  p * 2);
                        }
                    }
                }
            ";

            var result = this.RunTest(code, new PrivateOnlyTraceLoggingFilter(), "First.MyClass::Main");
            result.Count.Should().Be(4);
            //result.ElementAt(0).ShouldBeTraceEnterInto("First.MyClass::CallMe", "param", "Hello", "param2", "Hello2", "paraInt", "42");
            //result.ElementAt(1).ShouldBeTraceLeaveFrom("First.MyClass::CallMe", "84");
            //result.ElementAt(2).ShouldBeTraceEnterInto("First.MyClass::CallMe", "param", "Ahoy", "param2", "Ahoy2", "paraInt", "43");
            //result.ElementAt(3).ShouldBeTraceLeaveFrom("First.MyClass::CallMe", "86");
        }
    }
}
