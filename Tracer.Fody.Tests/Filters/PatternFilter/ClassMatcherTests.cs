using System;
using System.Collections.Generic;
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
        public void InvalidKeywordFails()
        {
            Action action = () => ClassMatcher.Create("[private]MyClass");
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
        public void AnyClassWithoutPrefixMatch()
        {
            var matcher = ClassMatcher.Create("*");
            matcher.IsMatch(GetTypeDefinition(typeof(MyClass))).Should().BeTrue();
            matcher.IsMatch(GetTypeDefinition(typeof(YourClass))).Should().BeTrue();
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

        [Test]
        public void PartialDefinitionGenericClass()
        {
            var matcher = ClassMatcher.Create("My*");
            matcher.IsMatch(GetTypeDefinition(typeof(MyGeneric<>))).Should().BeTrue();
        }

        [Test]
        public void PartialDefinitionGenericInstanceClass()
        {
            var matcher = ClassMatcher.Create("My*");
            matcher.IsMatch(GetGenericTypeDefinition(typeof(MyGeneric<>), typeof(string))).Should().BeTrue();
        }

        [Test]
        public void SortTest_LongerNameFirst()
        {
            var matcher1 = ClassMatcher.Create("MyClass");
            var matcher2 = ClassMatcher.Create("MyCl");
            var list = new List<ClassMatcher> { matcher2, matcher1 };
            list.Sort();
            list[0].Should().Be(matcher1);
            list[1].Should().Be(matcher2);
        }

        [Test]
        public void SortTest_ScopeDefsDoesntMatter()
        {
            var matcher1 = ClassMatcher.Create("MyClass");
            var matcher2 = ClassMatcher.Create("[public]MyCl");
            var list = new List<ClassMatcher> { matcher2, matcher1 };
            list.Sort();
            list[0].Should().Be(matcher1);
            list[1].Should().Be(matcher2);
        }

        [Test]
        public void SortTest_LessQuestionMarksFirst()
        {
            var matcher1 = ClassMatcher.Create("MyClas?");
            var matcher2 = ClassMatcher.Create("MyCla??");
            var list = new List<ClassMatcher> { matcher2, matcher1 };
            list.Sort();
            list[0].Should().Be(matcher1);
            list[1].Should().Be(matcher2);
        }

        [Test]
        public void SortTest_LongerNameFirstIfBothContainsStar()
        {
            var matcher1 = ClassMatcher.Create("My*Class");
            var matcher2 = ClassMatcher.Create("*MyCl");
            var list = new List<ClassMatcher> { matcher2, matcher1 };
            list.Sort();
            list[0].Should().Be(matcher1);
            list[1].Should().Be(matcher2);
        }

        [Test]
        public void SortTest_NameWithoutStarFirst()
        {
            var matcher1 = ClassMatcher.Create("MyClass");
            var matcher2 = ClassMatcher.Create("MyClassIsLongerButContains*");
            var list = new List<ClassMatcher> { matcher2, matcher1 };
            list.Sort();
            list[0].Should().Be(matcher1);
            list[1].Should().Be(matcher2);
        }

        private TypeDefinition GetTypeDefinition(Type runtimeType)
        {
            var asmDef = AssemblyDefinition.ReadAssembly(runtimeType.Module.FullyQualifiedName);
            var types = asmDef.MainModule.GetAllTypes();
            return types.FirstOrDefault(it => it.FullName == runtimeType.FullName);
        }

        private TypeDefinition GetGenericTypeDefinition(Type genericType, Type paramType)
        {
            var asmDef = AssemblyDefinition.ReadAssembly(genericType.Module.FullyQualifiedName);
            var types = asmDef.MainModule.GetAllTypes();
            var type = types.FirstOrDefault(it => it.FullName == genericType.FullName);
            var genType = type.MakeGenericInstanceType(asmDef.MainModule.ImportReference(paramType)).Resolve();
            return genType;
        }
    }

    public class MyGeneric<T> { }

    public class MyClass { }

    public class YourClass { }

    public struct MyStruct { }

    public class MyOtherClass { }

    class InternalClass { }

    class MyInternalClass { }
}
