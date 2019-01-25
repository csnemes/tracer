using System;
using System.Linq;
using FluentAssertions;
using Mono.Cecil;
using Mono.Cecil.Rocks;
using NUnit.Framework;
using Tracer.Fody.Filters.PatternFilter;

namespace Tracer.Fody.Tests.Filters.PatternFilter
{
    public class ClassMatcherTests
    {
        [Test]
        public void EmptyClassNameFails()
        {
            Action action = () => ClassMatcher.Create("[]");
            action.Should().Throw<ArgumentException>();
        }

        [Test]
        public void FullDefinitionWithoutPrefixMatch()
        {
            var matcher = ClassMatcher.Create("MyClass");
            matcher.IsMatch(GetTypeDefinition(typeof(MyClass))).Should().BeTrue();
            matcher.IsMatch(GetTypeDefinition(typeof(YourClass))).Should().BeFalse();
        }

        [Test]
        public void FullDefinitionWithoutPrefixMatch_MatchCaseInsensitive()
        {
            var matcher = ClassMatcher.Create("myclass");
            matcher.IsMatch(GetTypeDefinition(typeof(MyClass))).Should().BeTrue();
            matcher.IsMatch(GetTypeDefinition(typeof(YourClass))).Should().BeFalse();
        }

        [Test]
        public void FullDefinitionWithoutPrefixMatch_MatchPublicAndInternal()
        {
            var matcher = ClassMatcher.Create("MyClass");
            matcher.IsMatch(GetTypeDefinition(typeof(MyClass))).Should().BeTrue();
            matcher.IsMatch(GetTypeDefinition(typeof(YourClass))).Should().BeFalse();
            matcher = ClassMatcher.Create("InternalClass");
            matcher.IsMatch(GetTypeDefinition(typeof(InternalClass))).Should().BeTrue();
        }

        [Test]
        public void PartialDefinitionWithoutPrefixMatch()
        {
            var matcher = ClassMatcher.Create("My*");
            matcher.IsMatch(GetTypeDefinition(typeof(MyClass))).Should().BeTrue();
            matcher.IsMatch(GetTypeDefinition(typeof(YourClass))).Should().BeFalse();
            matcher.IsMatch(GetTypeDefinition(typeof(MyOtherClass))).Should().BeTrue();
            matcher.IsMatch(GetTypeDefinition(typeof(MyStruct))).Should().BeTrue();
            matcher.IsMatch(GetTypeDefinition(typeof(MyInternalClass))).Should().BeTrue();
        }

        [Test]
        public void PartialDefinitionWithoutPrefixMatchAtEnd()
        {
            var matcher = ClassMatcher.Create("*Class");
            matcher.IsMatch(GetTypeDefinition(typeof(MyClass))).Should().BeTrue();
            matcher.IsMatch(GetTypeDefinition(typeof(YourClass))).Should().BeTrue();
            matcher.IsMatch(GetTypeDefinition(typeof(MyOtherClass))).Should().BeTrue();
            matcher.IsMatch(GetTypeDefinition(typeof(MyStruct))).Should().BeFalse();
            matcher.IsMatch(GetTypeDefinition(typeof(MyInternalClass))).Should().BeTrue();
        }

        [Test]
        public void PartialDefinitionVisibilityPrefix_PublicOnly()
        {
            var matcher = ClassMatcher.Create("[public]My*");
            matcher.IsMatch(GetTypeDefinition(typeof(MyClass))).Should().BeTrue();
            matcher.IsMatch(GetTypeDefinition(typeof(MyOtherClass))).Should().BeTrue();
            matcher.IsMatch(GetTypeDefinition(typeof(MyStruct))).Should().BeTrue();
            matcher.IsMatch(GetTypeDefinition(typeof(MyInternalClass))).Should().BeFalse();
        }

        [Test]
        public void PartialDefinitionVisibilityPrefix_InternalOnly()
        {
            var matcher = ClassMatcher.Create("[internal]My*");
            matcher.IsMatch(GetTypeDefinition(typeof(MyClass))).Should().BeFalse();
            matcher.IsMatch(GetTypeDefinition(typeof(MyOtherClass))).Should().BeFalse();
            matcher.IsMatch(GetTypeDefinition(typeof(MyStruct))).Should().BeFalse();
            matcher.IsMatch(GetTypeDefinition(typeof(MyInternalClass))).Should().BeTrue();
        }

 private TypeDefinition GetTypeDefinition(Type runtimeType)
        {
            var asmDef = AssemblyDefinition.ReadAssembly(runtimeType.Module.FullyQualifiedName);
            var types = asmDef.MainModule.GetAllTypes();
            return types.FirstOrDefault(it => it.FullName == runtimeType.FullName);
        }
    }

    public class MyClass { }

    public class YourClass { }

    public struct MyStruct { }

    public class MyOtherClass { }

    class InternalClass { }

    class MyInternalClass { }
}
