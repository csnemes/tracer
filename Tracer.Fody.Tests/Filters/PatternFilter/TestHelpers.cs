using System;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Rocks;

namespace Tracer.Fody.Tests.Filters.PatternFilter
{
    public static class TestHelpers
    {
        public static MethodDefinition GetMethodDefinition(Type runtimeType, string methodName)
        {
            var asmDef = AssemblyDefinition.ReadAssembly(runtimeType.Module.FullyQualifiedName);
            var types = asmDef.MainModule.GetAllTypes();
            var type = types.FirstOrDefault(it => it.FullName == runtimeType.FullName);
            return type.GetMethods().FirstOrDefault(it => it.Name.Equals(methodName));
        }
    }
}
