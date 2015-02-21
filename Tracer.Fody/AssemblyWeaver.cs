using System.Collections.Generic;
using System.IO;
using System.Threading;
using Mono.Cecil;
using Mono.Cecil.Pdb;
using Tracer.Fody.Weavers;

namespace Tracer.Fody
{
    /// <summary>
    /// Class that can be used to execute weaving on a single assembly. 
    /// </summary>
    public static class AssemblyWeaver
    {
        /// <summary>
        /// Weaves the tracer to a single assembly. It adds a trace enter and trace leave call to all methods defined by the filter.
        /// It also replaces static Log calls to logger instance calls and extends the call parameters with method name information.
        /// Use the configuration to specify exact weaver behavior, for example to define the actual logging library/methods.  
        /// </summary>
        /// <param name="assemblyPath">Path to the assembly to be weaved</param>
        /// <param name="configuration">An instance of <see cref="TraceLoggingConfiguration"/>.</param>
        /// <example>
        /// Given a class with a method where Log is a static class
        /// public MyClass
        /// {
        ///     public int MyMethod(string inputString, int inputInteger)
        ///     {
        ///         ...
        ///         Log.Warning("Something suspicious");
        ///         ...
        ///         return result;
        ///     }
        /// } 
        /// 
        /// The rewriter will create the following (assuming that the trace with method call time is turned on)
        /// public MyClass
        /// {
        ///     private static LogAdapter _log = LogManager.GetLogger(typeof(MyClass));
        /// 
        ///     public int MyMethod(string inputString, int inputInteger)
        ///     {
        ///         _log.TraceEnter("int MyMethod(string, int)", new[] {"inputString", "inputInteger"}, new[] { inputString, inputInteger });
        ///         var startTick = Stopwatch.GetTimestamp();
        ///          ...
        ///         _log.LogWarning("int MyMethod(string, int)","Something suspicious");
        ///         ...
        /// 
        ///         _log.TraceLeave("int MyMethod(string, int)", Stopwatch.GetTimestamp() - startTick, result);
        ///         return result;
        ///     }
        /// } 
        /// </example>
        public static void Execute(string assemblyPath, TraceLoggingConfiguration configuration)
        {
            //get module definition
            ModuleDefinition moduleDef = null;

            var pdbFile = Path.ChangeExtension(assemblyPath, "pdb");
            var hasPdb = File.Exists(pdbFile);

            if (hasPdb)
            {
                using (var symbolStream = File.OpenRead(pdbFile))
                {
                    moduleDef = ModuleDefinition.ReadModule(assemblyPath, new ReaderParameters
                        {
                            AssemblyResolver = new DefaultAssemblyResolver(),
                            ReadSymbols = true,
                            SymbolReaderProvider = new PdbReaderProvider(),
                            SymbolStream = symbolStream
                        });
                }
            }
            else
            {
                moduleDef = ModuleDefinition.ReadModule(assemblyPath);
            }

            //execute weaving
            ModuleLevelWeaver.Execute(configuration, moduleDef);

            //write back the results
            moduleDef.Write(assemblyPath, new WriterParameters
                    {
                        WriteSymbols = hasPdb,
                        SymbolWriterProvider = new PdbWriterProvider()
                    });
        }
    }
}
