using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;
using NUnit.Framework;
using Tracer.Fody.Tests.Func.MockLoggers;
using Tracer.Fody.Weavers;

namespace Tracer.Fody.Tests.Func
{
    [TestFixture]
    public class ExecuteWeaveOnExamples
    {
        [Test, Explicit, Category("manual")]
        public void WeaveMyApplication()
        {
            var config = TraceLoggingConfiguration.New
                .WithFilter(new PublicMethodsFilter())
                .WithAdapterAssembly(typeof(Tracer.Log4net.Log).Assembly.GetName().FullName)
                .WithLogManager(typeof(Tracer.Log4net.Adapters.LogManagerAdapter).FullName)
                .WithLogger(typeof(Tracer.Log4net.Adapters.LoggerAdapter).FullName)
                .WithStaticLogger(typeof(Tracer.Log4net.Log).FullName);

            AssemblyWeaver.Execute("..\\..\\..\\TestApplication\\bin\\debug\\TestApplication.exe", config);
        }

        private class PublicMethodsFilter : ITraceLoggingFilter
        {
            public bool ShouldAddTrace(MethodDefinition definition)
            {
                return definition.IsPublic;
            }
        }
    }
}
