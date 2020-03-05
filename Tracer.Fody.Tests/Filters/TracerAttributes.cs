using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// ReSharper disable once CheckNamespace
namespace TracerAttributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Constructor | AttributeTargets.Struct, AllowMultiple = true, Inherited = true)]
    public class TraceOn : Attribute
    {
        public TraceTarget Target { get; set; }

        public OtherParameter OtherParameter { get; set; }

        public bool IncludeArguments { get; set; }

        public TraceOn()
        {}

        public TraceOn(TraceTarget traceTarget)
        {
            Target = traceTarget;
        }
    }

    public enum OtherParameter
    {
        ParamOne,
        ParamTwo
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property |
                    AttributeTargets.Parameter |
                    AttributeTargets.Constructor | AttributeTargets.Struct, AllowMultiple = true, Inherited = true)]
    public class NoTrace : Attribute
    {
    }

    public enum TraceTarget
    {
        Public,
        Internal,
        Protected,
        Private
    }

    /// <summary>
    /// This attribute excludes the logging of the return value of the marked method
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class NoReturnTrace : Attribute
    {
    }
}
