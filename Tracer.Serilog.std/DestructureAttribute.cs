using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tracer.Serilog
{
    /// <summary>
    /// Marker attribute for destructuring
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class DestructureAttribute : Attribute
    {
    }

    /// <summary>
    /// Used for adding destructured types to the loggers list
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class DestructureTypeAttribute : Attribute
    {
        public DestructureTypeAttribute(Type typeToDestructure)
        {
            TypeToDestructure = typeToDestructure;
        }

        public Type TypeToDestructure { get; }
    }
}
