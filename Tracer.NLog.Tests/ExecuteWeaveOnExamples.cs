using Mono.Cecil;
using NUnit.Framework;
using Tracer.Fody;
using Tracer.Fody.Filters;
using Tracer.Fody.Helpers;
using Tracer.Fody.Weavers;
using System;


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

            string src = @"D:\Work\TestNlogAutoLogger3\TestApplication.NLog\bin\Debug\TestApplication.NLog.exe";
            string dst = @"D:\Work\TestNlogAutoLogger3\TestApplication.NLog\bin\Debug\TestApplication.NLog224.exe";
            System.IO.File.Copy(src, dst, true);
            System.IO.Directory.SetCurrentDirectory(System.IO.Path.GetDirectoryName(dst));
            AssemblyWeaver.Execute(dst, config);
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
