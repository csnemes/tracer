using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using FluentAssertions;
using Mono.Cecil;
using Mono.Cecil.Rocks;
using NUnit.Framework;
using Tracer.Fody.Filters.PatternFilter;

namespace Tracer.Fody.Tests.Filters.PatternFilter
{
    public class PatternDefinitionTests
    {
        [Test]
        public void FullSpecifiedMatches()
        {
            var def = PatternDefinition.BuildUpDefinition("Mynamespace.MyClass.MyMethod", true);
            def.IsMatching(GetMethodDefinition(typeof(MyNamespace.MyClass), "MyMethod")).Should().BeTrue();
            def.IsMatching(GetMethodDefinition(typeof(MyNamespace.MyClass), "OtherMethod")).Should().BeFalse();
        }

        [Test]
        public void FullSpecifiedMultiNamespaceMatches()
        {
            var def = PatternDefinition.BuildUpDefinition("MyNamespace.Inner.AndMore.MyClass.MyMethod", true);
            def.IsMatching(GetMethodDefinition(typeof(MyNamespace.Inner.AndMore.MyClass), "MyMethod")).Should().BeTrue();
            def.IsMatching(GetMethodDefinition(typeof(MyNamespace.Inner.AndMore.MyClass), "OtherMethod")).Should().BeFalse();
        }

        [Test]
        public void MultiNamespaceUsingStar()
        {
            var def = PatternDefinition.BuildUpDefinition("MyNamespace.*.AndMore.MyClass.MyMethod", true);
            def.IsMatching(GetMethodDefinition(typeof(MyNamespace.Inner.AndMore.MyClass), "MyMethod")).Should().BeTrue();
            def.IsMatching(GetMethodDefinition(typeof(MyNamespace.Other.AndMore.MyClass), "MyMethod")).Should().BeTrue();
            def.IsMatching(GetMethodDefinition(typeof(MyNamespace.MyClass), "MyMethod")).Should().BeFalse();
        }

        [Test]
        public void MultiNamespaceTwoDottedAtEndMatches()
        {
            var def = PatternDefinition.BuildUpDefinition("MyNamespace..MyClass.MyMethod", true);
            def.IsMatching(GetMethodDefinition(typeof(MyNamespace.Inner.AndMore.MyClass), "MyMethod")).Should().BeTrue();
            def.IsMatching(GetMethodDefinition(typeof(MyNamespace.Inner.AndMore.MyClass), "OtherMethod")).Should().BeFalse();
        }

        [Test]
        public void MultiNamespaceTwoDottedAtBeginningMatches()
        {
            var def = PatternDefinition.BuildUpDefinition("..MyClass.MyMethod", true);
            def.IsMatching(GetMethodDefinition(typeof(MyNamespace.Inner.AndMore.MyClass), "MyMethod")).Should().BeTrue();
            def.IsMatching(GetMethodDefinition(typeof(MyNamespace.Inner.AndMore.MyClass), "OtherMethod")).Should().BeFalse();
        }

        [Test]
        public void MatchEverything()
        {
            var def = PatternDefinition.BuildUpDefinition("..*.*", true);
            def.IsMatching(GetMethodDefinition(typeof(MyNamespace.Inner.AndMore.MyClass), "MyMethod")).Should().BeTrue();
            def.IsMatching(GetMethodDefinition(typeof(MyNamespace.Inner.AndMore.MyClass), "OtherMethod")).Should().BeTrue();
            def.IsMatching(GetMethodDefinition(typeof(MyNamespace.Other.AndMore.MyClass), "OtherMethod")).Should().BeTrue();
            def.IsMatching(GetMethodDefinition(typeof(MyNamespace.OtherClass), "MyMethod")).Should().BeTrue();
        }

        [Test]
        public void MatchEveryPublicClass()
        {
            var def = PatternDefinition.BuildUpDefinition("..[public]*.*", true);
            def.IsMatching(GetMethodDefinition(typeof(MyNamespace.Inner.AndMore.MyClass), "MyMethod")).Should().BeTrue();
            def.IsMatching(GetMethodDefinition(typeof(MyNamespace.MyClass), "MyMethod")).Should().BeTrue();
            def.IsMatching(GetMethodDefinition(typeof(MyNamespace.OtherClass), "MyMethod")).Should().BeFalse();
        }

        [Test]
        public void ParsingParameters()
        {
            var def = PatternDefinition.ParseFromConfig(new XElement("On", new XAttribute("pattern", "..*.*"), new XAttribute("logParam", true),
                 new XAttribute("otherParam", "Test")), true);

            def.Parameters.Count.Should().Be(2);
            def.Parameters.Keys.Should().Contain("logParam");
            def.Parameters.Keys.Should().Contain("otherParam");
            def.Parameters["logParam"].Should().Be("true");
            def.Parameters["otherParam"].Should().Be("Test");
        }

        [Test]
        public void ParsingParametersNoParameters()
        {
            var def = PatternDefinition.ParseFromConfig(new XElement("On", new XAttribute("pattern", "..*.*")), true);

            def.Parameters.Count.Should().Be(0);
        }

        [Test]
        public void SortTest()
        {
            var def1 = PatternDefinition.BuildUpDefinition("MyNs.Inner.Other.[public]MyClass.MyMethod", true);
            var def2 = PatternDefinition.BuildUpDefinition("MyNs.Inner.Other.[public]MyClass.My*", true);
            var def3 = PatternDefinition.BuildUpDefinition("MyNs.Inner.Other.My*.MyMethod", true);
            var def4 = PatternDefinition.BuildUpDefinition("MyNs.Inner.Other.*.*", true);
            var def5 = PatternDefinition.BuildUpDefinition("MyNs..Other.*.*", true);
            var def6 = PatternDefinition.BuildUpDefinition("..*.*", true);

            var list = new List<PatternDefinition> { def6, def4, def1, def3, def5, def2 };
            list.Sort();
            list[0].Should().Be(def1);
            list[1].Should().Be(def2);
            list[2].Should().Be(def3);
            list[3].Should().Be(def4);
            list[4].Should().Be(def5);
            list[5].Should().Be(def6);
        }


        private MethodDefinition GetMethodDefinition(Type runtimeType, string methodName)
        {
            var asmDef = AssemblyDefinition.ReadAssembly(runtimeType.Module.FullyQualifiedName);
            var types = asmDef.MainModule.GetAllTypes();
            var type = types.FirstOrDefault(it => it.FullName == runtimeType.FullName);
            return type.GetMethods().FirstOrDefault(it => it.Name.Equals(methodName));
        }
    }
}

namespace MyNamespace
{
    public class MyClass
    {
        public void MyMethod() { }

        private void OtherMethod() { }
    }

    class OtherClass
    {
        public void MyMethod() { }

        private void OtherMethod() { }
    }
}

namespace MyNamespace.Inner.AndMore
{
    public class MyClass
    {
        public void MyMethod() { }

        private void OtherMethod() { }
    }
}

namespace MyNamespace.Other.AndMore
{
    public class MyClass
    {
        public void MyMethod() { }

        private void OtherMethod() { }
    }
}