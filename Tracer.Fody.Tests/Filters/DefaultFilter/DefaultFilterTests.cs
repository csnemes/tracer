using System;
using System.Linq;
using System.Xml.Linq;
using FluentAssertions;
using NUnit.Framework;
using Tracer.Fody.Filters;
using Tracer.Fody.Filters.DefaultFilter;
using Tracer.Fody.Weavers;

namespace Tracer.Fody.Tests.Filters.DefaultFilter
{
    [TestFixture]
    public class DefaultFilterTests : TestBase
    {
        [Test]
        public void Parse_MostBasicConfiguration()
        {
            var result = Fody.Filters.DefaultFilter.DefaultFilter.ParseConfig(XElement.Parse(@"<root>
                <TraceOn class=""public"" method =""public"" />
            </root>").Descendants()).ToList();

            result.Count.Should().Be(1);
            result[0].Should().BeOfType<AssemblyLevelTraceOnDefinition>();
        }

        [Test]
        public void Parse_MultiElement_Configuration()
        {
            var result = Fody.Filters.DefaultFilter.DefaultFilter.ParseConfig(XElement.Parse(@"<root>
                <TraceOn class=""public"" method =""public"" />
                <TraceOn namespace=""rootnamespace"" class=""public"" method =""public"" />
                <NoTrace namespace=""rootnamespace.other"" />
            </root>").Descendants()).ToList();

            result.Count.Should().Be(3);
            result[0].Should().BeOfType<AssemblyLevelTraceOnDefinition>();
            result[1].Should().BeOfType<AssemblyLevelTraceOnDefinition>();
            result[2].Should().BeOfType<AssemblyLevelNoTraceDefinition>();
        }

        [Test]
        public void Creation_MultiElementConfig()
        {
            var filter = new Fody.Filters.DefaultFilter.DefaultFilter(XElement.Parse(@"<root>
                <TraceOn class=""public"" method =""public"" />
                <TraceOn namespace=""rootnamespace"" class=""public"" method =""public"" />
                <NoTrace namespace=""rootnamespace.other"" />
            </root>").Descendants());

            string code = @"
                using TracerAttributes;

                namespace rootnamespace
                {
                    public class MyClass
                    {
                        public void PublicMethod()
                        {}

                        internal void InternalMethod()
                        {}

                        protected void ProtectedMethod()
                        {}

                        private void PrivateMethod()
                        {}
                    }
                }

                namespace rootnamespace.other
                {
                    public class OtherClass
                    {
                        public void OtherPublicMethod()
                        {}
                    }
                }

                namespace rootnamespace.another
                {
                    public class AnotherClass
                    {
                        public void AnotherPublicMethod()
                        {}
                    }
                }
            ";

            var publicMethodDef = GetMethodDefinition(code, "PublicMethod");
            var internalMethodDef = GetMethodDefinition(code, "InternalMethod");

            filter.ShouldAddTrace(publicMethodDef).Should().BeTrue();
            filter.ShouldAddTrace(internalMethodDef).Should().BeFalse();

            var otherPublicMethodDef = GetMethodDefinition(code, "OtherPublicMethod");
            filter.ShouldAddTrace(otherPublicMethodDef).Should().BeFalse();

            var anotherPublicMethodDef = GetMethodDefinition(code, "AnotherPublicMethod");
            filter.ShouldAddTrace(anotherPublicMethodDef).Should().BeTrue();
        }


        [Test]
        public void AssemblyLevelSpecification_PublicClass_PublicFilter()
        {
            string code = @"
                using TracerAttributes;

                namespace First
                {
                    public class MyClass
                    {
                        public void PublicMethod()
                        {}

                        internal void InternalMethod()
                        {}

                        protected void ProtectedMethod()
                        {}

                        private void PrivateMethod()
                        {}
                    }
                }
            ";

            var publicMethodDef = GetMethodDefinition(code, "PublicMethod");
            var internalMethodDef = GetMethodDefinition(code, "InternalMethod");
            var protectedMethodDef = GetMethodDefinition(code, "ProtectedMethod");
            var privateMethodDef = GetMethodDefinition(code, "PrivateMethod");
            var filter = GetDefaultFilter(TraceTargetVisibility.Public, TraceTargetVisibility.Public);
            filter.ShouldAddTrace(publicMethodDef).Should().BeTrue("public");
            filter.ShouldAddTrace(internalMethodDef).Should().BeFalse("internal");
            filter.ShouldAddTrace(protectedMethodDef).Should().BeFalse("protected");
            filter.ShouldAddTrace(privateMethodDef).Should().BeFalse("private");
        }

        [Test]
        public void AssemblyLevelSpecification_PublicClass_AllFilter()
        {
            string code = @"
                using TracerAttributes;

                namespace First
                {
                    public class MyClass
                    {
                        public void PublicMethod()
                        {}

                        internal void InternalMethod()
                        {}

                        protected void ProtectedMethod()
                        {}

                        private void PrivateMethod()
                        {}
                    }
                }
            ";

            var publicMethodDef = GetMethodDefinition(code, "PublicMethod");
            var internalMethodDef = GetMethodDefinition(code, "InternalMethod");
            var protectedMethodDef = GetMethodDefinition(code, "ProtectedMethod");
            var privateMethodDef = GetMethodDefinition(code, "PrivateMethod");
            var filter = GetDefaultFilter(TraceTargetVisibility.Public, TraceTargetVisibility.All);
            filter.ShouldAddTrace(publicMethodDef).Should().BeTrue("public");
            filter.ShouldAddTrace(internalMethodDef).Should().BeTrue("internal");
            filter.ShouldAddTrace(protectedMethodDef).Should().BeTrue("protected");
            filter.ShouldAddTrace(privateMethodDef).Should().BeTrue("private");
        }

        [Test]
        public void AssemblyLevelSpecification_InternalClass_PublicFilter()
        {
            string code = @"
                using TracerAttributes;

                namespace First
                {
                    internal class MyClass
                    {
                        public void PublicMethod()
                        {}

                        internal void InternalMethod()
                        {}

                        protected void ProtectedMethod()
                        {}

                        private void PrivateMethod()
                        {}
                    }
                }
            ";

            var publicMethodDef = GetMethodDefinition(code, "PublicMethod");
            var internalMethodDef = GetMethodDefinition(code, "InternalMethod");
            var protectedMethodDef = GetMethodDefinition(code, "ProtectedMethod");
            var privateMethodDef = GetMethodDefinition(code, "PrivateMethod");
            var filter = GetDefaultFilter(TraceTargetVisibility.Public, TraceTargetVisibility.All);
            filter.ShouldAddTrace(publicMethodDef).Should().BeFalse("public");
            filter.ShouldAddTrace(internalMethodDef).Should().BeFalse("internal");
            filter.ShouldAddTrace(protectedMethodDef).Should().BeFalse("protected");
            filter.ShouldAddTrace(privateMethodDef).Should().BeFalse("private");
        }

        [Test]
        public void AssemblyLevelSpecification_InternalClass_AllFilter()
        {
            string code = @"
                using TracerAttributes;

                namespace First
                {
                    internal class MyClass
                    {
                        public void PublicMethod()
                        {}

                        internal void InternalMethod()
                        {}

                        protected void ProtectedMethod()
                        {}

                        private void PrivateMethod()
                        {}
                    }
                }
            ";

            var publicMethodDef = GetMethodDefinition(code, "PublicMethod");
            var internalMethodDef = GetMethodDefinition(code, "InternalMethod");
            var protectedMethodDef = GetMethodDefinition(code, "ProtectedMethod");
            var privateMethodDef = GetMethodDefinition(code, "PrivateMethod");
            var filter = GetDefaultFilter(TraceTargetVisibility.All, TraceTargetVisibility.ProtectedOrMoreVisible);
            filter.ShouldAddTrace(publicMethodDef).Should().BeTrue("public");
            filter.ShouldAddTrace(internalMethodDef).Should().BeTrue("internal");
            filter.ShouldAddTrace(protectedMethodDef).Should().BeTrue("protected");
            filter.ShouldAddTrace(privateMethodDef).Should().BeFalse("private");
        }

        [Test]
        public void MethodLevelTraceOn_Overrides_AssemblyLevel()
        {
            string code = @"
                using TracerAttributes;

                namespace First
                {
                    public class MyClass
                    {
                        [TraceOn]
                        private void MyMethod()
                        {}
                    }
                }
            ";

            var methodDef = GetMethodDefinition(code, "MyMethod");
            var filter = GetDefaultFilter(TraceTargetVisibility.Public, TraceTargetVisibility.Public);
            filter.ShouldAddTrace(methodDef).Should().BeTrue();
        }

        [Test]
        public void MethodLevelTraceOn_Overrides_ClassLevel()
        {
            string code = @"
                using TracerAttributes;

                namespace First
                {
                    [NoTrace]
                    public class MyClass
                    {
                        [TraceOn]
                        private void MyMethod()
                        {}
                    }
                }
            ";

            var methodDef = GetMethodDefinition(code, "MyMethod");
            var filter = GetDefaultFilter(TraceTargetVisibility.Public, TraceTargetVisibility.Public);
            filter.ShouldAddTrace(methodDef).Should().BeTrue();
        }

        [Test]
        public void MethodLevelNoTrace_Overrides_AssemblyLevel()
        {
            string code = @"
                using TracerAttributes;

                namespace First
                {
                    public class MyClass
                    {
                        [NoTrace]
                        public void MyMethod()
                        {}
                    }
                }
            ";

            var methodDef = GetMethodDefinition(code, "MyMethod");
            var filter = GetDefaultFilter(TraceTargetVisibility.Public, TraceTargetVisibility.Public);
            filter.ShouldAddTrace(methodDef).Should().BeFalse();
        }

        [Test]
        public void ClassLevelTraceOn_Overrides_AssemblyLevel_PrivateLevel()
        {
            string code = @"
                using TracerAttributes;

                namespace First
                {
                    [TraceOn(Target=TraceTarget.Private)]
                    public class MyClass
                    {
                        public void PublicMethod()
                        {}

                        internal void InternalMethod()
                        {}

                        protected void ProtectedMethod()
                        {}

                        private void PrivateMethod()
                        {}
                    }
                }
            ";

            var publicMethodDef = GetMethodDefinition(code, "PublicMethod");
            var internalMethodDef = GetMethodDefinition(code, "InternalMethod");
            var protectedMethodDef = GetMethodDefinition(code, "ProtectedMethod");
            var privateMethodDef = GetMethodDefinition(code, "PrivateMethod");
            var filter = GetDefaultFilter(TraceTargetVisibility.Public, TraceTargetVisibility.Public);
            filter.ShouldAddTrace(publicMethodDef).Should().BeTrue("public");
            filter.ShouldAddTrace(internalMethodDef).Should().BeTrue("internal");
            filter.ShouldAddTrace(protectedMethodDef).Should().BeTrue("protected");
            filter.ShouldAddTrace(privateMethodDef).Should().BeTrue("private");
        }

        [Test]
        public void ClassLevelTraceOn_Overrides_AssemblyLevel_InternalLevel()
        {
            string code = @"
                using TracerAttributes;

                namespace First
                {
                    [TraceOn(TraceTarget.Internal)]
                    public class MyClass
                    {
                        public void PublicMethod()
                        {}

                        internal void InternalMethod()
                        {}

                        protected void ProtectedMethod()
                        {}

                        private void PrivateMethod()
                        {}
                    }
                }
            ";

            var publicMethodDef = GetMethodDefinition(code, "PublicMethod");
            var internalMethodDef = GetMethodDefinition(code, "InternalMethod");
            var protectedMethodDef = GetMethodDefinition(code, "ProtectedMethod");
            var privateMethodDef = GetMethodDefinition(code, "PrivateMethod");
            var filter = GetDefaultFilter(TraceTargetVisibility.Public, TraceTargetVisibility.All);
            filter.ShouldAddTrace(publicMethodDef).Should().BeTrue("public");
            filter.ShouldAddTrace(internalMethodDef).Should().BeTrue("internal");
            filter.ShouldAddTrace(protectedMethodDef).Should().BeFalse("protected");
            filter.ShouldAddTrace(privateMethodDef).Should().BeFalse("private");
        }

        [Test]
        public void ClassLevelTraceOn_Overrides_AssemblyLevel_PublicLevel()
        {
            string code = @"
                using TracerAttributes;

                namespace First
                {
                    [TraceOn(TraceTarget.Public)]
                    public class MyClass
                    {
                        public void PublicMethod()
                        {}

                        internal void InternalMethod()
                        {}

                        protected void ProtectedMethod()
                        {}

                        private void PrivateMethod()
                        {}
                    }
                }
            ";

            var publicMethodDef = GetMethodDefinition(code, "PublicMethod");
            var internalMethodDef = GetMethodDefinition(code, "InternalMethod");
            var protectedMethodDef = GetMethodDefinition(code, "ProtectedMethod");
            var privateMethodDef = GetMethodDefinition(code, "PrivateMethod");
            var filter = GetDefaultFilter(TraceTargetVisibility.Public, TraceTargetVisibility.All);
            filter.ShouldAddTrace(publicMethodDef).Should().BeTrue("public");
            filter.ShouldAddTrace(internalMethodDef).Should().BeFalse("internal");
            filter.ShouldAddTrace(protectedMethodDef).Should().BeFalse("protected");
            filter.ShouldAddTrace(privateMethodDef).Should().BeFalse("private");
        }

        [Test]
        public void ClassLevelNoTrace_Overrides_AssemblyLevel()
        {
            string code = @"
                using TracerAttributes;

                namespace First
                {
                    [NoTrace]
                    public class MyClass
                    {
                        public void PublicMethod()
                        {}

                        internal void InternalMethod()
                        {}

                        protected void ProtectedMethod()
                        {}

                        private void PrivateMethod()
                        {}
                    }
                }
            ";

            var publicMethodDef = GetMethodDefinition(code, "PublicMethod");
            var internalMethodDef = GetMethodDefinition(code, "InternalMethod");
            var protectedMethodDef = GetMethodDefinition(code, "ProtectedMethod");
            var privateMethodDef = GetMethodDefinition(code, "PrivateMethod");
            var filter = GetDefaultFilter(TraceTargetVisibility.Public, TraceTargetVisibility.All);
            filter.ShouldAddTrace(publicMethodDef).Should().BeFalse("public");
            filter.ShouldAddTrace(internalMethodDef).Should().BeFalse("internal");
            filter.ShouldAddTrace(protectedMethodDef).Should().BeFalse("protected");
            filter.ShouldAddTrace(privateMethodDef).Should().BeFalse("private");
        }

        [Test]
        public void NestedClassLevelNoTrace_Overrides_AssemblyLevel()
        {
            string code = @"
                using TracerAttributes;

                namespace First
                {
                    [NoTrace]
                    public class MyClass
                    {
                        public class InnerClass
                        {
                            public void PublicMethod()
                            {}

                            public void PublicMethod2()
                            {}
                        }
                    }
                }
            ";

            var publicMethodDef = GetMethodDefinition(code, "PublicMethod");
            var publicMethodDef2 = GetMethodDefinition(code, "PublicMethod");
            var filter = GetDefaultFilter(TraceTargetVisibility.Public, TraceTargetVisibility.All);
            filter.ShouldAddTrace(publicMethodDef).Should().BeFalse("public");
            filter.ShouldAddTrace(publicMethodDef2).Should().BeFalse("public");
        }

        [Test]
        public void NestedClassLevelTraceOn_Overrides_AssemblyLevel()
        {
            string code = @"
                using TracerAttributes;

                namespace First
                {
                    [TraceOn(TraceTarget.Protected)]
                    public class MyClass
                    {
                        public class InnerClass
                        {
                            public void PublicMethod()
                            {}

                            internal void InternalMethod()
                            {}

                            protected void ProtectedMethod()
                            {}

                            private void PrivateMethod()
                            {}
                        }
                    }
                }
            ";

            var publicMethodDef = GetMethodDefinition(code, "PublicMethod");
            var internalMethodDef = GetMethodDefinition(code, "InternalMethod");
            var protectedMethodDef = GetMethodDefinition(code, "ProtectedMethod");
            var privateMethodDef = GetMethodDefinition(code, "PrivateMethod");
            var filter = GetDefaultFilter(TraceTargetVisibility.Public, TraceTargetVisibility.All);
            filter.ShouldAddTrace(publicMethodDef).Should().BeTrue("public");
            filter.ShouldAddTrace(internalMethodDef).Should().BeTrue("internal");
            filter.ShouldAddTrace(protectedMethodDef).Should().BeTrue("protected");
            filter.ShouldAddTrace(privateMethodDef).Should().BeFalse("private");
        }

        [Test]
        public void ParseConfig_DefaultConfig_Parsed()
        {
            var input = new XElement("Tracer",
                new XElement("TraceOn", new XAttribute("class", "public"), new XAttribute("method", "public"))
                );

            var parseResult = Fody.Filters.DefaultFilter.DefaultFilter.ParseConfig(input.Descendants()).ToList();
            parseResult.Count.Should().Be(1);
            parseResult[0].Should().BeOfType<AssemblyLevelTraceOnDefinition>();
            ((AssemblyLevelTraceOnDefinition)parseResult[0]).TargetClass.Should().Be(TraceTargetVisibility.Public);
            ((AssemblyLevelTraceOnDefinition)parseResult[0]).TargetMethod.Should().Be(TraceTargetVisibility.Public);
        }

        [Test]
        public void ParseConfig_PrivateConfig_Parsed()
        {
            var input = new XElement("Tracer",
                new XElement("TraceOn", new XAttribute("class", "internal"), new XAttribute("method", "private"))
                );

            var parseResult = Fody.Filters.DefaultFilter.DefaultFilter.ParseConfig(input.Descendants()).ToList();
            parseResult.Count.Should().Be(1);
            parseResult[0].Should().BeOfType<AssemblyLevelTraceOnDefinition>();
            ((AssemblyLevelTraceOnDefinition)parseResult[0]).TargetClass.Should().Be(TraceTargetVisibility.InternalOrMoreVisible);
            ((AssemblyLevelTraceOnDefinition)parseResult[0]).TargetMethod.Should().Be(TraceTargetVisibility.All);
        }

        [Test]
        public void ParseConfig_MissingAttribute_Throws()
        {
            var input = new XElement("Tracer",
                new XElement("TraceOn", new XAttribute("method", "private"))
                );

            Action runParse = () => Fody.Filters.DefaultFilter.DefaultFilter.ParseConfig(input.Descendants());
            runParse.Should().Throw<Exception>();
        }

        [Test]
        public void ParseConfig_WrongAttributeValue_Throws()
        {
            var input = new XElement("Tracer",
                new XElement("TraceOn", new XAttribute("class", "wrongvalue"), new XAttribute("method", "private"))
                );

            Action runParse = () => Fody.Filters.DefaultFilter.DefaultFilter.ParseConfig(input.Descendants());
            runParse.Should().Throw<Exception>();
        }


        private ITraceLoggingFilter GetDefaultFilter(TraceTargetVisibility classTarget,
            TraceTargetVisibility methodTarget)
        {
            var config = new[] {new AssemblyLevelTraceOnDefinition(NamespaceScope.All, classTarget, methodTarget)};
            return new Fody.Filters.DefaultFilter.DefaultFilter(config);
        }
    }
}
