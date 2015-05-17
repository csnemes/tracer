using Mono.Cecil;
using NUnit.Framework;
using Tracer.Fody;
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
                .WithAdapterAssembly(typeof(Tracer.Log4Net.Log).Assembly.GetName().FullName)
                .WithLogManager(typeof(Tracer.Log4Net.Adapters.LogManagerAdapter).FullName)
                .WithLogger(typeof(Tracer.Log4Net.Adapters.LoggerAdapter).FullName)
                .WithStaticLogger(typeof(Tracer.Log4Net.Log).FullName);

            AssemblyWeaver.Execute("..\\..\\..\\TestApplication\\bin\\debug\\TestApplication.exe", config);
        }

        private class PublicMethodsFilter : ITraceLoggingFilter
        {
            public bool ShouldAddTrace(MethodDefinition definition)
            {
                return true;
            }
        }
    }
}
