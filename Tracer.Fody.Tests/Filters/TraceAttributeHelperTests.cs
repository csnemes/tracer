using System;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Mono.Cecil;
using Mono.Cecil.Rocks;
using NUnit.Framework;
using Tracer.Fody.Filters;
using TracerAttributes;

namespace Tracer.Fody.Tests.Filters
{
    [TestFixture]
    public class TraceAttributeHelperTests
    {
        [Test]
        public void TestCustomAttributeVisibilityParsing()
        {
            var attr = GetTypeDefinition("TestClass").CustomAttributes[0];
            var result = TraceAttributeHelper.GetTargetVisibilityFromAttribute(attr);
            result.Should().Be(TraceTargetVisibility.All);
        }

        [Test]
        public void TestCustomAttributeVisibilityParsing2()
        {
            var attr = GetTypeDefinition("TestClass2").CustomAttributes[0];
            var result = TraceAttributeHelper.GetTargetVisibilityFromAttribute(attr);
            result.Should().Be(TraceTargetVisibility.All);
        }

        private TypeDefinition GetTypeDefinition(string typeName)
        {
            var dllLocation = new Uri(Assembly.GetExecutingAssembly().CodeBase).AbsolutePath;

            using (var moduleDef = ModuleDefinition.ReadModule(dllLocation))
            {
                return moduleDef.GetAllTypes()
                    .FirstOrDefault(typeDefinition => typeDefinition.Name.Equals(typeName, StringComparison.OrdinalIgnoreCase));
            }
        }
    }

    [TraceOn(TraceTarget.Private, IncludeArguments = true)]
    public class TestClass
    {
    }

    [TraceOn(IncludeArguments = true, Target = TraceTarget.Private)]
    public class TestClass2
    {
    }

}
