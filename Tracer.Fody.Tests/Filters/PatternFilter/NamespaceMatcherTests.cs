using FluentAssertions;
using NUnit.Framework;
using Tracer.Fody.Filters.PatternFilter;

namespace Tracer.Fody.Tests.Filters.PatternFilter
{
    public class NamespaceMatcherTests
    {
        [Test]
        public void FullSpecMatch()
        {
            var matcher = new NamespaceMatcher("MyNamespace.Inner");
            matcher.IsMatch("MyNamespace.Inner").Should().BeTrue();
            matcher.IsMatch("MyNamespace.inner").Should().BeTrue();
            matcher.IsMatch("MyNamespace.Other").Should().BeFalse();
        }

        [Test]
        public void QuestionMarkMatch()
        {
            var matcher = new NamespaceMatcher("MyNam?space.Inner");
            matcher.IsMatch("MyNamespace.Inner").Should().BeTrue();
            matcher.IsMatch("MyNamfspace.inner").Should().BeTrue();
            matcher.IsMatch("MyNam.space.Other").Should().BeFalse();
        }

        [Test]
        public void MultipleQuestionMarksMatch()
        {
            var matcher = new NamespaceMatcher("MyNam?space.Inn?r");
            matcher.IsMatch("MyNamespace.Inner").Should().BeTrue();
            matcher.IsMatch("MyNamfspace.innEr").Should().BeTrue();
            matcher.IsMatch("MyNamfspace.innfr").Should().BeTrue();
            matcher.IsMatch("MyNamespace.Inn?r").Should().BeFalse();
        }

        [Test]
        public void StarMarkMatch()
        {
            var matcher = new NamespaceMatcher("My*.Inner");
            matcher.IsMatch("MyNamespace.Inner").Should().BeTrue();
            matcher.IsMatch("My.Inner").Should().BeTrue();
            matcher.IsMatch("MyOther.inner").Should().BeTrue();
            matcher.IsMatch("MyNamespace.Other").Should().BeFalse();
            matcher.IsMatch("MyNam.Space.Inner").Should().BeFalse();
            matcher.IsMatch("YourNamespace.Inner").Should().BeFalse();
            matcher.IsMatch("MyNamespace.Other.Inner").Should().BeFalse();
            matcher.IsMatch("My.Namespace.Inner").Should().BeFalse();
        }

        [Test]
        public void DoubleDotMatch()
        {
            var matcher = new NamespaceMatcher("MyNamespace..Inner");
            matcher.IsMatch("MyNamespace.Inner").Should().BeTrue();
            matcher.IsMatch("MyNamespace.Some.inner").Should().BeTrue();
            matcher.IsMatch("MyNamespace.Some.Other.Inner").Should().BeTrue();
            matcher.IsMatch("MyNamespace.Other").Should().BeFalse();
            matcher.IsMatch("MyNamespace.Some.Other").Should().BeFalse();
        }

        [Test]
        public void MultipleDoubleDotMatch()
        {
            var matcher = new NamespaceMatcher("MyNamespace..Other..Inner");
            matcher.IsMatch("MyNamespace.Other.Inner").Should().BeTrue();
            matcher.IsMatch("MyNamespace.Some.Other.Inner").Should().BeTrue();
            matcher.IsMatch("MyNamespace.Some.Other.Some.Some.Inner").Should().BeTrue();
            matcher.IsMatch("MyNamespace.Other.Some.Inner").Should().BeTrue();
            matcher.IsMatch("MyNamespace.Other").Should().BeFalse();
            matcher.IsMatch("MyNamespace.Some.Other").Should().BeFalse();
            matcher.IsMatch("MyNamespace.Inner").Should().BeFalse();
            matcher.IsMatch("MyNamespace.Inner.Other").Should().BeFalse();
        }
    }
}
