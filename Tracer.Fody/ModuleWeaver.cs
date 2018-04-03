using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Fody;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Tracer.Fody.Filters;
using Tracer.Fody.Helpers;
using Tracer.Fody.Weavers;

namespace Tracer.Fody
{
    /// <summary>
    /// Class that links fody to the real weaver
    /// </summary>
    public class ModuleWeaver : BaseModuleWeaver, IWeavingLogger
    {
        /// <summary>
        /// Weaves the tracer to a the module specified in <see cref="ModuleDefinition"/> property. It adds a trace enter and trace leave call to all methods defined by the filter.
        /// It also replaces static Log calls to logger instance calls and extends the call parameters with method name information.
        /// It uses the configuration to identify the exact weaver behavior.
        /// </summary>
        public override void Execute()
        {
            WeavingLog.SetLogger(this);

            var parser = FodyConfigParser.Parse(Config);

            if (parser.IsErroneous)
            {
                LogError(parser.Error);
            }
            else
            {
                ModuleLevelWeaver.Execute(parser.Result, ModuleDefinition);
            }
        }

        public override IEnumerable<string> GetAssembliesForScanning()
        {
            yield break;
        }

        private TraceLoggingConfiguration ParseConfiguration(XElement config)
        {
            return null;
        }

        void IWeavingLogger.LogDebug(string message)
        {
            LogDebug(message);
        }

        void IWeavingLogger.LogInfo(string message)
        {
            LogInfo(message);
        }

        void IWeavingLogger.LogWarning(string message)
        {
            LogWarning(message);
        }

        void IWeavingLogger.LogError(string message)
        {
            LogError(message);
        }
    }
}
