using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Mono.Cecil;
using Mono.Cecil.Rocks;
using NUnit.Framework;
using Tracer.Fody.Filters.PatternFilter;

namespace Tracer.Fody.Tests.Filters.PatternFilter
{
    public class MemberMatcherTests
    {
        static MemberMatcherTests() { }

        public MemberMatcherTests()
        {}

        protected MemberMatcherTests(int inp)
        {}

        [Test]
        public void EmptyMemberNameFails()
        {
            Action action = () => MemberMatcher.Create("[]");
            action.Should().Throw<ArgumentException>();
        }

        [Test]
        public void ConstructorMatching()
        {
            var matcher = MemberMatcher.Create("[constructor]*");
            matcher.IsMatch(GetMethodDefinition(nameof(MyPropPrivate))).Should().BeFalse();
            matcher.IsMatch(GetConstructorDefinition(true)).Should().BeTrue();
            matcher.IsMatch(GetConstructorDefinition(false)).Should().BeTrue();
            matcher.IsMatch(GetStaticConstructorDefinition()).Should().BeTrue();
        }

        [Test]
        public void StaticOnlyConstructorMatching()
        {
            var matcher = MemberMatcher.Create("[static|constructor]*");
            matcher.IsMatch(GetConstructorDefinition(true)).Should().BeFalse();
            matcher.IsMatch(GetMethodDefinition(nameof(MyPropPrivate))).Should().BeFalse();
            matcher.IsMatch(GetStaticConstructorDefinition()).Should().BeTrue();
        }

        [Test]
        public void ConstructorMatchingWithVisibility()
        {
            var matcher = MemberMatcher.Create("[protected|constructor]*");
            matcher.IsMatch(GetMethodDefinition(nameof(MyPropPrivate))).Should().BeFalse();
            matcher.IsMatch(GetConstructorDefinition(true)).Should().BeFalse();
            matcher.IsMatch(GetConstructorDefinition(false)).Should().BeTrue();
            matcher.IsMatch(GetStaticConstructorDefinition()).Should().BeTrue();
        }

        [Test]
        public void PartialDefinitionAllVisibilityInstance()
        {
            var matcher = MemberMatcher.Create("My*");
            matcher.IsMatch(GetMethodDefinition(nameof(MyPropPrivate))).Should().BeTrue();
            matcher.IsMatch(GetMethodDefinition(nameof(MyPropProtected))).Should().BeTrue();
            matcher.IsMatch(GetMethodDefinition(nameof(MyPropInternal))).Should().BeTrue();
            matcher.IsMatch(GetMethodDefinition(nameof(MyPropPublic))).Should().BeTrue();
            matcher.IsMatch(GetMethodDefinition(nameof(MyMethodPrivate))).Should().BeTrue();
            matcher.IsMatch(GetMethodDefinition(nameof(MyMethodProtected))).Should().BeTrue();
            matcher.IsMatch(GetMethodDefinition(nameof(MyMethodInternal))).Should().BeTrue();
            matcher.IsMatch(GetMethodDefinition(nameof(MyMethodProtectedInternal))).Should().BeTrue();
            matcher.IsMatch(GetMethodDefinition(nameof(MyMethodPublic))).Should().BeTrue();
            matcher.IsMatch(GetMethodDefinition(nameof(OtherPropPublic))).Should().BeFalse();
            matcher.IsMatch(GetMethodDefinition(nameof(OtherMethodPublic))).Should().BeFalse();
        }

        [Test]
        public void PartialDefinitionAllVisibilityStatic()
        {
            var matcher = MemberMatcher.Create("My*");
            matcher.IsMatch(GetMethodDefinition(nameof(MyPropPrivateStatic))).Should().BeTrue();
            matcher.IsMatch(GetMethodDefinition(nameof(MyPropProtectedStatic))).Should().BeTrue();
            matcher.IsMatch(GetMethodDefinition(nameof(MyPropInternalStatic))).Should().BeTrue();
            matcher.IsMatch(GetMethodDefinition(nameof(MyPropPublicStatic))).Should().BeTrue();
            matcher.IsMatch(GetMethodDefinition(nameof(MyMethodPrivateStatic))).Should().BeTrue();
            matcher.IsMatch(GetMethodDefinition(nameof(MyMethodProtectedStatic))).Should().BeTrue();
            matcher.IsMatch(GetMethodDefinition(nameof(MyMethodInternalStatic))).Should().BeTrue();
            matcher.IsMatch(GetMethodDefinition(nameof(MyMethodProtectedInternalStatic))).Should().BeTrue();
            matcher.IsMatch(GetMethodDefinition(nameof(MyMethodPublicStatic))).Should().BeTrue();
            matcher.IsMatch(GetMethodDefinition(nameof(OtherPropPublicStatic))).Should().BeFalse();
            matcher.IsMatch(GetMethodDefinition(nameof(OtherMethodPublicStatic))).Should().BeFalse();
        }

        [Test]
        public void PartialDefinitionPublicVisibility()
        {
            var matcher = MemberMatcher.Create("[public]My*");
            matcher.IsMatch(GetMethodDefinition(nameof(MyPropPrivate))).Should().BeFalse();
            matcher.IsMatch(GetMethodDefinition(nameof(MyPropProtected))).Should().BeFalse();
            matcher.IsMatch(GetMethodDefinition(nameof(MyPropInternal))).Should().BeFalse();
            matcher.IsMatch(GetMethodDefinition(nameof(MyPropPublic))).Should().BeTrue();
            matcher.IsMatch(GetMethodDefinition(nameof(MyMethodPrivate))).Should().BeFalse();
            matcher.IsMatch(GetMethodDefinition(nameof(MyMethodProtected))).Should().BeFalse();
            matcher.IsMatch(GetMethodDefinition(nameof(MyMethodInternal))).Should().BeFalse();
            matcher.IsMatch(GetMethodDefinition(nameof(MyMethodProtectedInternal))).Should().BeFalse();
            matcher.IsMatch(GetMethodDefinition(nameof(MyMethodPublic))).Should().BeTrue();
            matcher.IsMatch(GetMethodDefinition(nameof(OtherPropPublic))).Should().BeFalse();
            matcher.IsMatch(GetMethodDefinition(nameof(OtherMethodPublic))).Should().BeFalse();
        }

        [Test]
        public void PartialDefinitionProtectedVisibility()
        {
            var matcher = MemberMatcher.Create("[protected]*Protected");
            matcher.IsMatch(GetMethodDefinition(nameof(MyMethodProtected))).Should().BeTrue();
            matcher.IsMatch(GetMethodDefinition(nameof(MyPropProtected))).Should().BeTrue();
            matcher.IsMatch(GetMethodDefinition(nameof(MyPropInternal))).Should().BeFalse();
        }

        [Test]
        public void PartialDefinitionStaticSetters()
        {
            var matcher = MemberMatcher.Create("[static|set]My*");
            matcher.IsMatch(GetMethodDefinition(nameof(MyPropPrivateStatic))).Should().BeTrue();
            matcher.IsMatch(GetMethodDefinition(nameof(MyPropProtectedStatic))).Should().BeTrue();
            matcher.IsMatch(GetMethodDefinition(nameof(MyPropInternalStatic))).Should().BeTrue();
            matcher.IsMatch(GetMethodDefinition(nameof(MyPropPublicStatic))).Should().BeTrue();
            matcher.IsMatch(GetMethodDefinition(nameof(MyPropPrivate))).Should().BeFalse();
            matcher.IsMatch(GetMethodDefinition(nameof(MyPropProtected))).Should().BeFalse();
            matcher.IsMatch(GetMethodDefinition(nameof(MyPropInternal))).Should().BeFalse();
            matcher.IsMatch(GetMethodDefinition(nameof(MyPropPublic))).Should().BeFalse();
            matcher.IsMatch(GetMethodDefinition(nameof(MyMethodPrivate))).Should().BeFalse();
            matcher.IsMatch(GetMethodDefinition(nameof(MyMethodProtected))).Should().BeFalse();
            matcher.IsMatch(GetMethodDefinition(nameof(MyMethodInternal))).Should().BeFalse();
            matcher.IsMatch(GetMethodDefinition(nameof(MyMethodProtectedInternal))).Should().BeFalse();
            matcher.IsMatch(GetMethodDefinition(nameof(MyMethodPublic))).Should().BeFalse();
            matcher.IsMatch(GetMethodDefinition(nameof(MyMethodPrivateStatic))).Should().BeFalse();
            matcher.IsMatch(GetMethodDefinition(nameof(MyMethodProtectedStatic))).Should().BeFalse();
            matcher.IsMatch(GetMethodDefinition(nameof(MyMethodInternalStatic))).Should().BeFalse();
            matcher.IsMatch(GetMethodDefinition(nameof(MyMethodProtectedInternalStatic))).Should().BeFalse();
            matcher.IsMatch(GetMethodDefinition(nameof(MyMethodPublicStatic))).Should().BeFalse();
        }

        [Test]
        public void PartialDefinitionIstancePrivate()
        {
            var matcher = MemberMatcher.Create("[instance|private]My*");
            matcher.IsMatch(GetMethodDefinition(nameof(MyPropPrivate))).Should().BeTrue();
            matcher.IsMatch(GetMethodDefinition(nameof(MyPropProtected))).Should().BeFalse();
            matcher.IsMatch(GetMethodDefinition(nameof(MyPropInternal))).Should().BeFalse();
            matcher.IsMatch(GetMethodDefinition(nameof(MyMethodPrivate))).Should().BeTrue();
            matcher.IsMatch(GetMethodDefinition(nameof(MyMethodProtected))).Should().BeFalse();
            matcher.IsMatch(GetMethodDefinition(nameof(MyPropPrivateStatic))).Should().BeFalse();
        }

        [Test]
        public void PartialDefinitioGenericMethod()
        {
            var matcher = MemberMatcher.Create("My*");
            matcher.IsMatch(GetMethodDefinition(nameof(MyGeneric))).Should().BeTrue();
        }

        [Test]
        public void FullDefinitionMatch()
        {
            var matcher = MemberMatcher.Create("MyPropPrivate");
            matcher.IsMatch(GetMethodDefinition(nameof(MyPropPrivate))).Should().BeTrue();
            matcher.IsMatch(GetMethodDefinition(nameof(MyPropProtected))).Should().BeFalse();
            matcher.IsMatch(GetMethodDefinition(nameof(MyPropPrivateStatic))).Should().BeFalse();
        }

        [Test]
        public void SortTest_LongerNameFirst()
        {
            var matcher1 = MemberMatcher.Create("MyClass");
            var matcher2 = MemberMatcher.Create("MyCl");
            var list = new List<MemberMatcher> { matcher2, matcher1 };
            list.Sort();
            list[0].Should().Be(matcher1);
            list[1].Should().Be(matcher2);
        }

        [Test]
        public void SortTest_ScopeDefsDoesntMatter()
        {
            var matcher1 = MemberMatcher.Create("MyClass");
            var matcher2 = MemberMatcher.Create("[public]MyCl");
            var list = new List<MemberMatcher> { matcher2, matcher1 };
            list.Sort();
            list[0].Should().Be(matcher1);
            list[1].Should().Be(matcher2);
        }

        [Test]
        public void SortTest_LessQuestionMarksFirst()
        {
            var matcher1 = MemberMatcher.Create("MyClas?");
            var matcher2 = MemberMatcher.Create("MyCla??");
            var list = new List<MemberMatcher> { matcher2, matcher1 };
            list.Sort();
            list[0].Should().Be(matcher1);
            list[1].Should().Be(matcher2);
        }

        [Test]
        public void SortTest_LongerNameFirstIfBothContainsStar()
        {
            var matcher1 = MemberMatcher.Create("My*Class");
            var matcher2 = MemberMatcher.Create("*MyCl");
            var list = new List<MemberMatcher> { matcher2, matcher1 };
            list.Sort();
            list[0].Should().Be(matcher1);
            list[1].Should().Be(matcher2);
        }

        [Test]
        public void SortTest_NameWithoutStarFirst()
        {
            var matcher1 = MemberMatcher.Create("MyClass");
            var matcher2 = MemberMatcher.Create("MyClassIsLongerButContains*");
            var list = new List<MemberMatcher> { matcher2, matcher1 };
            list.Sort();
            list[0].Should().Be(matcher1);
            list[1].Should().Be(matcher2);
        }


        MethodDefinition GetConstructorDefinition(bool publicVisibility)
        {
            var asmDef = AssemblyDefinition.ReadAssembly(typeof(MemberMatcherTests).Module.FullyQualifiedName);
            var type = asmDef.MainModule.GetType(typeof(MemberMatcherTests).FullName);
            var methods = type.GetConstructors();
            return methods.First(it => !it.IsStatic && ((publicVisibility && it.IsPublic) || (!publicVisibility && !it.IsPublic)) );
        }

        MethodDefinition GetStaticConstructorDefinition()
        {
            var asmDef = AssemblyDefinition.ReadAssembly(typeof(MemberMatcherTests).Module.FullyQualifiedName);
            var type = asmDef.MainModule.GetType(typeof(MemberMatcherTests).FullName);
            return type.GetStaticConstructor();
        }

        MethodDefinition GetMethodDefinition(string methodName)
        {
            var asmDef = AssemblyDefinition.ReadAssembly(typeof(MemberMatcherTests).Module.FullyQualifiedName);
            var type = asmDef.MainModule.GetType(typeof(MemberMatcherTests).FullName);
            var methods = type.GetMethods();
            return methods.First(it => NameMatches(it, methodName));
        }

        private bool NameMatches(MethodDefinition methodDefinition, string name)
        {
            if (methodDefinition.Name.Equals(name)) return true;
            if (methodDefinition.IsSetter || methodDefinition.IsGetter)
            {
                return methodDefinition.Name.Substring(4).Equals(name);
            }
            return false;
        }

        public void MyGeneric<T>() { }

        public void OtherMethodPublic() { }
        public string OtherPropPublic { get; set; }
        public static void OtherMethodPublicStatic() { }
        public static string OtherPropPublicStatic { get; set; }

        private void MyMethodPrivate() { }
        protected void MyMethodProtected() { }
        internal void MyMethodInternal() { }
        protected internal void MyMethodProtectedInternal() { }
        public void MyMethodPublic() { }

        private string MyPropPrivate { get; set; }
        protected string MyPropProtected { get; set; }
        internal string MyPropInternal { get; set; }
        public string MyPropPublic { get; set; }

        private static void MyMethodPrivateStatic() { }
        protected static void MyMethodProtectedStatic() { }
        internal static void MyMethodInternalStatic() { }
        protected internal static void MyMethodProtectedInternalStatic() { }
        public static void MyMethodPublicStatic() { }

        private static string MyPropPrivateStatic { get; set; }
        protected static string MyPropProtectedStatic { get; set; }
        internal static string MyPropInternalStatic { get; set; }
        public static string MyPropPublicStatic { get; set; }
    }
}
