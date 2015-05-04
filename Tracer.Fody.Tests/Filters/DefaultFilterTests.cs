using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using Tracer.Fody.Filters;
using Tracer.Fody.Weavers;

namespace Tracer.Fody.Tests.Filters
{
    [TestFixture]
    public class DefaultFilterTests : TestBase
    {
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
            var filter = GetDefaultFilter(DefaultFilter.TraceTargetVisibility.Public, DefaultFilter.TraceTargetVisibility.Public);
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
            var filter = GetDefaultFilter(DefaultFilter.TraceTargetVisibility.Public, DefaultFilter.TraceTargetVisibility.All);
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
            var filter = GetDefaultFilter(DefaultFilter.TraceTargetVisibility.Public, DefaultFilter.TraceTargetVisibility.All);
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
            var filter = GetDefaultFilter(DefaultFilter.TraceTargetVisibility.All, DefaultFilter.TraceTargetVisibility.ProtectedOrMoreVisible);
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
            var filter = GetDefaultFilter(DefaultFilter.TraceTargetVisibility.Public, DefaultFilter.TraceTargetVisibility.Public);
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
            var filter = GetDefaultFilter(DefaultFilter.TraceTargetVisibility.Public, DefaultFilter.TraceTargetVisibility.Public);
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
            var filter = GetDefaultFilter(DefaultFilter.TraceTargetVisibility.Public, DefaultFilter.TraceTargetVisibility.Public);
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
            var filter = GetDefaultFilter(DefaultFilter.TraceTargetVisibility.Public, DefaultFilter.TraceTargetVisibility.Public);
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
            var filter = GetDefaultFilter(DefaultFilter.TraceTargetVisibility.Public, DefaultFilter.TraceTargetVisibility.All);
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
            var filter = GetDefaultFilter(DefaultFilter.TraceTargetVisibility.Public, DefaultFilter.TraceTargetVisibility.All);
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
            var filter = GetDefaultFilter(DefaultFilter.TraceTargetVisibility.Public, DefaultFilter.TraceTargetVisibility.All);
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
            var filter = GetDefaultFilter(DefaultFilter.TraceTargetVisibility.Public, DefaultFilter.TraceTargetVisibility.All);
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
            var filter = GetDefaultFilter(DefaultFilter.TraceTargetVisibility.Public, DefaultFilter.TraceTargetVisibility.All);
            filter.ShouldAddTrace(publicMethodDef).Should().BeTrue("public");
            filter.ShouldAddTrace(internalMethodDef).Should().BeTrue("internal");
            filter.ShouldAddTrace(protectedMethodDef).Should().BeTrue("protected");
            filter.ShouldAddTrace(privateMethodDef).Should().BeFalse("private");
        }

        private ITraceLoggingFilter GetDefaultFilter(DefaultFilter.TraceTargetVisibility classTarget,
            DefaultFilter.TraceTargetVisibility methodTarget)
        {
            var config = new[] {new DefaultFilter.AssemblyLevelTraceDefinition(classTarget, methodTarget)};
            return new DefaultFilter(config);
        }
    }
}
