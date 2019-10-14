using Mono.Cecil;
using NUnit.Framework;
using Tracer.Fody;
using Tracer.Fody.Filters;
using Tracer.Fody.Helpers;
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
                .WithAdapterAssembly(typeof(Tracer.NLog.Log).Assembly.GetName().FullName)
                .WithLogManager(typeof(Tracer.NLog.Adapters.LogManagerAdapter).FullName)
                .WithLogger(typeof(Tracer.NLog.Adapters.LoggerAdapter).FullName)
                .WithStaticLogger(typeof(Tracer.NLog.Log).FullName);

            AssemblyWeaver.Execute("..\\..\\..\\TestApplication.NLog\\bin\\debug\\TestApplication.NLog.exe", config);
        }

        private class PublicMethodsFilter : ITraceLoggingFilter
        {
            public FilterResult ShouldAddTrace(MethodDefinition definition)
            {
                return new FilterResult(true);
            }

            public void LogFilterInfo(IWeavingLogger weavingLogger)
            {
            }

        }
    }
}
