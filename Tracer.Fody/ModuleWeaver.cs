using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
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
    public class ModuleWeaver : IWeavingLogger
    {
        public ModuleDefinition ModuleDefinition { get; set; }


        public void Execute()
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

        private TraceLoggingConfiguration ParseConfiguration(XElement config)
        {
            return null;
        }

        // Will contain the full element XML from FodyWeavers.xml. OPTIONAL
        public XElement Config { get; set; }

        // Will log an MessageImportance.Normal message to MSBuild. OPTIONAL
        public Action<string> LogDebug { get; set; }

        // Will log an MessageImportance.High message to MSBuild. OPTIONAL
        public Action<string> LogInfo { get; set; }

        // Will log an warning message to MSBuild. OPTIONAL
        public Action<string> LogWarning { get; set; }

        // Will log an error message to MSBuild. OPTIONAL
        public Action<string> LogError { get; set; }

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
