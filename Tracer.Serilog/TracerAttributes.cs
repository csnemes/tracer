using System;

// ReSharper disable once CheckNamespace
namespace TracerAttributes
{
    /// <summary>
    /// This attributes specifies that the marked element(s) should be traced.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Constructor | AttributeTargets.Struct, AllowMultiple = true, Inherited = true)]
    public class TraceOn : Attribute
    {
        public TraceTarget Target { get; set; }

        public TraceOn()
        {}

        public TraceOn(TraceTarget traceTarget)
        {
            Target = traceTarget;
        }
    }

    /// <summary>
    /// This attribute excludes the marked element from tracing.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Parameter | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Constructor | AttributeTargets.Struct, AllowMultiple = true, Inherited = true)]
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
