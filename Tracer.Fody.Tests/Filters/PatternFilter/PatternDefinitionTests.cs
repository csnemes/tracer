using System.Collections.Generic;
using System.Xml.Linq;
using FluentAssertions;
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
            def.IsMatching(TestHelpers.GetMethodDefinition(typeof(MyNamespace.MyClass), "MyMethod")).Should().BeTrue();
            def.IsMatching(TestHelpers.GetMethodDefinition(typeof(MyNamespace.MyClass), "OtherMethod")).Should().BeFalse();
        }

        [Test]
        public void FullSpecifiedMultiNamespaceMatches()
        {
            var def = PatternDefinition.BuildUpDefinition("MyNamespace.Inner.AndMore.MyClass.MyMethod", true);
            def.IsMatching(TestHelpers.GetMethodDefinition(typeof(MyNamespace.Inner.AndMore.MyClass), "MyMethod")).Should().BeTrue();
            def.IsMatching(TestHelpers.GetMethodDefinition(typeof(MyNamespace.Inner.AndMore.MyClass), "OtherMethod")).Should().BeFalse();
        }

        [Test]
        public void MultiNamespaceUsingStar()
        {
            var def = PatternDefinition.BuildUpDefinition("MyNamespace.*.AndMore.MyClass.MyMethod", true);
            def.IsMatching(TestHelpers.GetMethodDefinition(typeof(MyNamespace.Inner.AndMore.MyClass), "MyMethod")).Should().BeTrue();
            def.IsMatching(TestHelpers.GetMethodDefinition(typeof(MyNamespace.Other.AndMore.MyClass), "MyMethod")).Should().BeTrue();
            def.IsMatching(TestHelpers.GetMethodDefinition(typeof(MyNamespace.MyClass), "MyMethod")).Should().BeFalse();
        }

        [Test]
        public void MultiNamespaceTwoDottedAtEndMatches()
        {
            var def = PatternDefinition.BuildUpDefinition("MyNamespace..MyClass.MyMethod", true);
            def.IsMatching(TestHelpers.GetMethodDefinition(typeof(MyNamespace.Inner.AndMore.MyClass), "MyMethod")).Should().BeTrue();
            def.IsMatching(TestHelpers.GetMethodDefinition(typeof(MyNamespace.Inner.AndMore.MyClass), "OtherMethod")).Should().BeFalse();
        }

        [Test]
        public void MultiNamespaceTwoDottedAtBeginningMatches()
        {
            var def = PatternDefinition.BuildUpDefinition("..MyClass.MyMethod", true);
            def.IsMatching(TestHelpers.GetMethodDefinition(typeof(MyNamespace.Inner.AndMore.MyClass), "MyMethod")).Should().BeTrue();
            def.IsMatching(TestHelpers.GetMethodDefinition(typeof(MyNamespace.Inner.AndMore.MyClass), "OtherMethod")).Should().BeFalse();
        }

        [Test]
        public void MatchEverything()
        {
            var def = PatternDefinition.BuildUpDefinition("..*.*", true);
            def.IsMatching(TestHelpers.GetMethodDefinition(typeof(MyNamespace.Inner.AndMore.MyClass), "MyMethod")).Should().BeTrue();
            def.IsMatching(TestHelpers.GetMethodDefinition(typeof(MyNamespace.Inner.AndMore.MyClass), "OtherMethod")).Should().BeTrue();
            def.IsMatching(TestHelpers.GetMethodDefinition(typeof(MyNamespace.Other.AndMore.MyClass), "OtherMethod")).Should().BeTrue();
            def.IsMatching(TestHelpers.GetMethodDefinition(typeof(MyNamespace.OtherClass), "MyMethod")).Should().BeTrue();
        }

        [Test]
        public void MatchEveryPublicClass()
        {
            var def = PatternDefinition.BuildUpDefinition("..[public]*.*", true);
            def.IsMatching(TestHelpers.GetMethodDefinition(typeof(MyNamespace.Inner.AndMore.MyClass), "MyMethod")).Should().BeTrue();
            def.IsMatching(TestHelpers.GetMethodDefinition(typeof(MyNamespace.MyClass), "MyMethod")).Should().BeTrue();
            def.IsMatching(TestHelpers.GetMethodDefinition(typeof(MyNamespace.OtherClass), "MyMethod")).Should().BeFalse();
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
        public void SortRestrictionTest()
        {
            var def1 = PatternDefinition.BuildUpDefinition("*", true);
            var def2 = PatternDefinition.BuildUpDefinition("..[public]*.[public|method]*", true);
            var def3 = PatternDefinition.BuildUpDefinition("..[public|internal]*.[public|method]*", true);

            var list = new List<PatternDefinition> { def1, def2, def3 };
            list.Sort();
            list[0].Should().BeSameAs(def3);
            list[1].Should().BeSameAs(def2);
            list[2].Should().BeSameAs(def1);
        }

        [Test]
        public void SortRestrictionOnMemberTest()
        {
            var def1 = PatternDefinition.BuildUpDefinition("*", true);
            var def2 = PatternDefinition.BuildUpDefinition("..*.[public|method]*", true);
            var def3 = PatternDefinition.BuildUpDefinition("..*.[public]*", true);

            var list = new List<PatternDefinition> { def1, def2, def3 };
            list.Sort();
            list[0].Should().BeSameAs(def2);
            list[1].Should().BeSameAs(def3);
            list[2].Should().BeSameAs(def1);
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
            list[0].Should().BeSameAs(def1);
            list[1].Should().BeSameAs(def2);
            list[2].Should().BeSameAs(def3);
            list[3].Should().BeSameAs(def4);
            list[4].Should().BeSameAs(def5);
            list[5].Should().BeSameAs(def6);
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