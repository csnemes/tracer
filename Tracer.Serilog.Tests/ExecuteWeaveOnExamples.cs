using Mono.Cecil;
using NUnit.Framework;
using Tracer.Fody;
using Tracer.Fody.Filters;
using Tracer.Fody.Weavers;


namespace Tracer.Log4Net.Tests
{
    [TestFixture]
    public class ExecuteWeaveOnExamples
    {
        [Test, Explicit, Category("manual")]
        public void WeaveMyApplication()
        {
            var config = TraceLoggingConfiguration.New
                .WithFilter(new PublicMethodsFilter())
                .WithAdapterAssembly(typeof(Tracer.Serilog.Log).Assembly.GetName().FullName)
                .WithLogManager(typeof(Tracer.Serilog.Adapters.LogManagerAdapter).FullName)
                .WithLogger(typeof(Tracer.Serilog.Adapters.LoggerAdapter).FullName)
                .WithStaticLogger(typeof(Tracer.Serilog.Log).FullName);

            AssemblyWeaver.Execute("..\\..\\..\\TestApplication.Serilog\\bin\\debug\\TestApplication.Serilog.exe", config);
        }

        private class PublicMethodsFilter : ITraceLoggingFilter
        {
            public FilterResult ShouldAddTrace(MethodDefinition definition)
            {
                return new FilterResult(true);
            }
        }
    }
}
