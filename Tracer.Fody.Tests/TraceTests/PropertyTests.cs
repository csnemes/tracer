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
    public class PropertyTests : TestBase
    {
        [Test]
        public void Test_Property_Getter()
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
                            var mc = new MyClass(); 
                            var i = mc.IntValue;
                        }

                        private int IntValue
                        {
                            get { return 1; }
                        }
                    }
                }
            ";

            var result = this.RunTest(code, new PrivateOnlyTraceLoggingFilter(), "First.MyClass::Main");
            result.Count.Should().Be(2);
            result.ElementAt(0).ShouldBeTraceEnterInto("First.MyClass::get_IntValue");
            result.ElementAt(1).ShouldBeTraceLeaveFrom("First.MyClass::get_IntValue", "1");
        }

        [Test]
        public void Test_Property_Setter()
        {
            string code = @"
                using System;
                using System.Diagnostics;

                namespace First
                {
                    public class MyClass
                    {
                        private int _intValue;

                        public static void Main()
                        {
                            var mc = new MyClass(); 
                            mc.IntValue = 2;
                        }

                        private int IntValue
                        {
                            set { _intValue = value; }
                        }
                    }
                }
            ";

            var result = this.RunTest(code, new PrivateOnlyTraceLoggingFilter(), "First.MyClass::Main");
            result.Count.Should().Be(2);
            result.ElementAt(0).ShouldBeTraceEnterInto("First.MyClass::set_IntValue", "value", "2");
            result.ElementAt(1).ShouldBeTraceLeaveFrom("First.MyClass::set_IntValue");
        }

        [Test]
        public void Test_AutoProperty()
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
                            var mc = new MyClass(); 
                            mc.FirstName = ""john"";
                            var i = mc.FirstName;
                        }

                        private string FirstName { get; set; }

                        private string LastName { get; set; }

                        private string FullName
                        {
                            get
                            {
                                return $""{ this.LastName} { this.FirstName}"";
                            }
                        }
                    }
                }
            ";

            var result = this.RunTest(code, new PrivateOnlyTraceLoggingFilter(), "First.MyClass::Main");
            result.Count.Should().Be(4);
            result.ElementAt(0).ShouldBeTraceEnterInto("First.MyClass::set_FirstName");
            result.ElementAt(1).ShouldBeTraceLeaveFrom("First.MyClass::set_FirstName");
            result.ElementAt(2).ShouldBeTraceEnterInto("First.MyClass::get_FirstName");
            result.ElementAt(3).ShouldBeTraceLeaveFrom("First.MyClass::get_FirstName", "john");
        }



    }
}
