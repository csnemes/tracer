using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Fody;
using Mono.Cecil;
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

            var parser = FodyConfigParser.Parse(Config, GetDefaultConfig());

            if (parser.IsErroneous)
            {
                LogError(parser.Error);
            }
            else
            {
                parser.Result.Filter.LogFilterInfo(this);
                ModuleLevelWeaver.Execute(parser.Result, ModuleDefinition);
            }
        }

        private XElement GetDefaultConfig()
        {
            try
            {
                var defaultConfigFile = Path.Combine(AddinDirectoryPath, "default.config");
                var xDoc = XDocument.Load(defaultConfigFile);
                LogDebug($"Found default config file at {defaultConfigFile}.");
                return xDoc.Root;
            }
            catch (Exception ex)
            {
                LogDebug($"Exception {ex} while trying to load default config.");
                return XElement.Parse("<Tracer />");
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
