using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;
using Mono.Cecil.Rocks;
using Tracer.Fody.Helpers;

namespace Tracer.Fody.Weavers
{
    /// <summary>
    /// Class responsible for adding trace entry and exit calls to methods specified by the filter and replacing static log calls.
    /// This is a module level weaver, separate instance should be created for each module.
    ///  </summary>
    internal class ModuleLevelWeaver : ILoggerAdapterMetadataScopeProvider
    {
        private readonly ModuleDefinition _moduleDefinition;
        private readonly TraceLoggingConfiguration _configuration;

        private ModuleLevelWeaver(TraceLoggingConfiguration configuration, ModuleDefinition moduleDefinition)
        {
            _configuration = configuration;
            _moduleDefinition = moduleDefinition;
        }

        /// <summary>
        /// Weaves the logging and tracing into the given module. Please note that the module itself is modified.
        /// Configuration is used to specify certain weaving behaviors and provide necessary input for the weaver
        /// </summary>
        /// <param name="configuration">Configuration information</param>
        /// <param name="moduleDefinition">Target module</param>

        public static void Execute(TraceLoggingConfiguration configuration, ModuleDefinition moduleDefinition)
        {
            try
            {
                WeavingLog.LogInfo("Tracer: Starts weaving.");
                var timer = Stopwatch.StartNew();
                var weaver = new ModuleLevelWeaver(configuration, moduleDefinition);
                weaver.InternalExecute();
                timer.Stop();
                WeavingLog.LogInfo(String.Format("Tracer: Weaving done in {0} ms.", timer.ElapsedMilliseconds));
            }
            catch (Exception ex)
            {
                WeavingLog.LogError(String.Format("Tracer: Weaving failed with {0}", ex));
                throw;
            }
        }

        private void InternalExecute()
        {
            var typeReferenceProvider = new TypeReferenceProvider(_configuration, this, _moduleDefinition);
            var methodReferenceProvider = new MethodReferenceProvider(typeReferenceProvider, _moduleDefinition);

            var factory = new TypeWeaverFactory(_configuration.Filter, typeReferenceProvider, methodReferenceProvider, _configuration.ShouldTraceConstructors,
                _configuration.ShouldTraceProperties);
            foreach (var type in _moduleDefinition.GetAllTypes())
            {
                var weaver = factory.Create(type);
                weaver.Execute();
            }
        }

        private IMetadataScope _loggerScope;

        public IMetadataScope GetLoggerAdapterMetadataScope()
        {
            if (_loggerScope == null)
            {
                //Check if reference to adapter assembly is present. If not, add it (we only look for the name, we assume that different versions are backward compatible)
                var loggerReference = _moduleDefinition.AssemblyReferences.FirstOrDefault(assRef => assRef.Name.Equals(_configuration.AssemblyNameReference.Name));
                if (loggerReference == null)
                {
                    loggerReference = _configuration.AssemblyNameReference;
                    _moduleDefinition.AssemblyReferences.Add(loggerReference);
                }
                _loggerScope = loggerReference;
            }
            return _loggerScope;
        }
    }
}
