using System;
using FluentAssertions;
using NUnit.Framework;
using Tracer.Fody.Filters.DefaultFilter;

namespace Tracer.Fody.Tests.Filters.DefaultFilter
{
    [TestFixture]
    public class NamespaceScopeTests
    {
        [Test]
        public void Parse_GeneralParsingTests()
        {
            var result = NamespaceScope.Parse("mynamespace");
            result.ToString().Should().Be("mynamespace");
            result = NamespaceScope.Parse("mynamespace.other");
            result.ToString().Should().Be("mynamespace.other");
            result = NamespaceScope.Parse("mynamespace.other.*");
            result.ToString().Should().Be("mynamespace.other.*");
            result = NamespaceScope.Parse("mynamespace.other+*");
            result.ToString().Should().Be("mynamespace.other+*");
        }

        [Test]
        public void ParseConfig_FailureTests()
        {
            Action runParse = () => NamespaceScope.Parse("my*name");
            runParse.Should().Throw<Exception>();
            runParse = () => NamespaceScope.Parse("");
            runParse.Should().Throw<Exception>().And.Message.Should().Contain("empty");
            runParse = () => NamespaceScope.Parse("my.other.");
            runParse.Should().Throw<Exception>();
            runParse = () => NamespaceScope.Parse("my.other*");
            runParse.Should().Throw<Exception>();
        }

        [Test]
        public void IsMatching_ExactMatch_NoMatch()
        {
            var scope = NamespaceScope.Parse("mynamespace.other");
            scope.IsMatching("mynamespace.othe").Should().BeFalse();
            scope.IsMatching("mynamespace.other2").Should().BeFalse();
            scope.IsMatching("mynamespace.other.deep").Should().BeFalse();
        }

        [Test]
        public void IsMatching_ExactMatch_Match()
        {
            var scope = NamespaceScope.Parse("mynamespace.other");
            scope.IsMatching("mynamespace.other").Should().BeTrue();
        }

        [Test]
        public void IsMatching_OnlyChildren_Same_NoMatch()
        {
            var scope = NamespaceScope.Parse("mynamespace.other.*");
            scope.IsMatching("mynamespace.other").Should().BeFalse();
        }

        [Test]
        public void IsMatching_OnlyChildren_Substring_NoMatch()
        {
            var scope = NamespaceScope.Parse("mynamespace.other.*");
            scope.IsMatching("mynamespace.other2").Should().BeFalse();
        }

        [Test]
        public void IsMatching_OnlyChildren_Overstring_NoMatch()
        {
            var scope = NamespaceScope.Parse("mynamespace.other.*");
            scope.IsMatching("mynamespace.othe").Should().BeFalse();
        }

        [Test]
        public void IsMatching_OnlyChildren_Child_Match()
        {
            var scope = NamespaceScope.Parse("mynamespace.other.*");
            scope.IsMatching("mynamespace.other.child").Should().BeTrue();
        }

        [Test]
        public void IsMatching_OnlyChildren_DeepChild_Match()
        {
            var scope = NamespaceScope.Parse("mynamespace.other.*");
            scope.IsMatching("mynamespace.other.child.deep").Should().BeTrue();
        }

        [Test]
        public void IsMatching_SelfAndChildren_Same_Match()
        {
            var scope = NamespaceScope.Parse("mynamespace.other+*");
            scope.IsMatching("mynamespace.other").Should().BeTrue();
        }

        [Test]
        public void IsMatching_SelfAndChildren_Substring_NoMatch()
        {
            var scope = NamespaceScope.Parse("mynamespace.other+*");
            scope.IsMatching("mynamespace.other2").Should().BeFalse();
        }

        [Test]
        public void IsMatching_SelfAndChildren_Overstring_NoMatch()
        {
            var scope = NamespaceScope.Parse("mynamespace.other+*");
            scope.IsMatching("mynamespace.othe").Should().BeFalse();
        }

        [Test]
        public void IsMatching_SelfAndChildren_Child_Match()
        {
            var scope = NamespaceScope.Parse("mynamespace.other+*");
            scope.IsMatching("mynamespace.other.child").Should().BeTrue();
        }

        [Test]
        public void IsMatching_SelfAndChildren_DeepChild_Match()
        {
            var scope = NamespaceScope.Parse("mynamespace.other+*");
            scope.IsMatching("mynamespace.other.child.deep").Should().BeTrue();
        }

        [Test]
        public void IsMatching_All_Match()
        {
            var scope = NamespaceScope.All;
            scope.IsMatching("mynamespace.other.child.deep").Should().BeTrue();
            scope.IsMatching("mynamespace").Should().BeTrue();
            scope.IsMatching("").Should().BeTrue();
        }

        [Test]
        public void Compare_Length_Tests()
        {
            var ns1 = NamespaceScope.Parse("mynamespace");
            var ns2 = NamespaceScope.Parse("mynamespace.other");
            var ns3 = NamespaceScope.Parse("mynamespace.other.deep");

            ns1.CompareTo(ns2).Should().Be(1);
            ns2.CompareTo(ns1).Should().Be(-1);
            ns1.CompareTo(ns3).Should().Be(1);
            ns3.CompareTo(ns1).Should().Be(-1);
            ns2.CompareTo(ns3).Should().Be(1);
            ns3.CompareTo(ns2).Should().Be(-1);
        }

        [Test]
        public void Compare_All_Tests()
        {
            var ns1 = NamespaceScope.Parse("mynamespace");
            var ns2 = NamespaceScope.Parse("mynamespace.other");

            ns1.CompareTo(NamespaceScope.All).Should().Be(-1);
            ns2.CompareTo(NamespaceScope.All).Should().Be(-1);
            NamespaceScope.All.CompareTo(ns1).Should().Be(1);
            NamespaceScope.All.CompareTo(ns2).Should().Be(1);
        }

        [Test]
        public void Compare_MatchType_Tests()
        {
            var ns1 = NamespaceScope.Parse("mynamespace");
            var ns2 = NamespaceScope.Parse("mynamespace.*");
            var ns3 = NamespaceScope.Parse("mynamespace+*");

            ns1.CompareTo(ns2).Should().Be(-1);
            ns2.CompareTo(ns1).Should().Be(1);
            ns1.CompareTo(ns3).Should().Be(-1);
            ns3.CompareTo(ns1).Should().Be(1);
            ns2.CompareTo(ns3).Should().Be(-1);
            ns3.CompareTo(ns2).Should().Be(1);
        }
    }
}
