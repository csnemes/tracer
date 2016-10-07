using System;

// ReSharper disable once CheckNamespace
namespace TracerAttributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
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

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
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

}
