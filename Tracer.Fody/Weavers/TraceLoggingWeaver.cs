using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;
using Mono.Cecil.Rocks;

namespace Tracer.Fody.Weavers
{
    /// <summary>
    /// Class responsible for adding trace entry and exit calls to methods specified by the filter. This is a module level weaver, separate instance should be created for each module
    /// </summary>
    internal class TraceLoggingWeaver : ILoggerScopeProvider
    {
        private readonly ModuleDefinition _moduleDefinition;
        private readonly TraceLoggingConfiguration _configuration;

        private TraceLoggingWeaver(TraceLoggingConfiguration configuration, ModuleDefinition moduleDefinition)
        {
            _configuration = configuration;
            _moduleDefinition = moduleDefinition;
        }

        public static void Execute(TraceLoggingConfiguration configuration, ModuleDefinition moduleDefinition)
        {
            var weaver = new TraceLoggingWeaver(configuration, moduleDefinition);
            weaver.InternalExecute();
        }

        private void InternalExecute()
        {
            foreach (var type in _moduleDefinition.GetAllTypes())
            {
                TypeWeaver.Execute(_configuration, this, type);
            }
        }

        private IMetadataScope _loggerScope;

        public IMetadataScope GetLoggerScope()
        {
            if (_loggerScope == null)
            {
                //Check if reference to HPWorks is present if not add it (we only look for the name, we assume that different versions are backward compatible)
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
