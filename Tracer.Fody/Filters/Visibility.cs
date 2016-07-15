using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace Tracer.Fody.Filters
{
    internal enum VisibilityLevel
    {
        Public = 0,
        Internal = 1,
        Protected = 2,
        Private = 3
    }

    internal enum TraceTargetVisibility
    {
        None = -1,
        Public = 0,
        InternalOrMoreVisible = 1,
        ProtectedOrMoreVisible = 2,
        All = 3
    }

    internal static class VisibilityHelper
    {
        public static VisibilityLevel GetTypeVisibilityLevel(TypeDefinition typeDefinition)
        {
            if (typeDefinition.IsNested)
            {
                if (typeDefinition.IsNestedPublic) return VisibilityLevel.Public;
                if (typeDefinition.IsNestedAssembly) return VisibilityLevel.Internal;
                if (typeDefinition.IsNestedFamilyOrAssembly) return VisibilityLevel.Internal; //protected internal
                if (typeDefinition.IsNestedFamily) return VisibilityLevel.Protected;
                return VisibilityLevel.Private;
            }
            if (typeDefinition.IsPublic) return VisibilityLevel.Public;
            return VisibilityLevel.Internal;
        }

        public static VisibilityLevel GetMethodVisibilityLevel(MethodDefinition methodDefinition)
        {
            if (methodDefinition.IsPublic) return VisibilityLevel.Public;
            if (methodDefinition.IsAssembly) return VisibilityLevel.Internal;
            if (methodDefinition.IsFamilyOrAssembly) return VisibilityLevel.Internal;
            if (methodDefinition.IsFamily) return VisibilityLevel.Protected;
            return VisibilityLevel.Private;
        }

    }
}
