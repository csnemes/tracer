using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tracer.Fody.Filters
{
    /// <summary>
    /// Comparer to compare two <see cref="AssemblyLevelTraceDefinition"/> instances.
    /// </summary>
    internal class AssemblyLevelTraceDefinitionComparer : IComparer<AssemblyLevelTraceDefinition>
    {
        public static readonly IComparer<AssemblyLevelTraceDefinition> Instance = new AssemblyLevelTraceDefinitionComparer();

        public int Compare(AssemblyLevelTraceDefinition x, AssemblyLevelTraceDefinition y)
        {             //x<y -> -1, x==y ->0, x>y ->1

            var nsComp = x.NamespaceScope.CompareTo(y.NamespaceScope);
            if (nsComp != 0) return nsComp;
            if (x is AssemblyLevelNoTraceDefinition) return -1;
            if (y is AssemblyLevelNoTraceDefinition) return 1;

            //both x and y are TraceOn defs
            var xClassLevel = (int)((AssemblyLevelTraceOnDefinition)x).TargetClass;
            var yClassLevel = (int)((AssemblyLevelTraceOnDefinition)y).TargetClass;

            if (xClassLevel != yClassLevel)
            {
                return xClassLevel - yClassLevel;
            }

            return ((int)((AssemblyLevelTraceOnDefinition)x).TargetMethod).CompareTo((int)((AssemblyLevelTraceOnDefinition)y).TargetMethod);
        }

    }
}
