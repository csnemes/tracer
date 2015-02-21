using System.Collections.Generic;
using System.IO;
using System.Threading;
using Mono.Cecil;
using Mono.Cecil.Pdb;
using Tracer.Fody.Weavers;

namespace Tracer.Fody
{
    public static class AssemblyWeaver
    {
        /// <summary>
        /// Weaves the tracer to a single assembly. It adds a trace enter and trace leave call to all methods defined by the filter.
        /// Use the configuration to specify exact weaver behavior, for example to define the actual logging library/methods.  
        /// </summary>
        /// <param name="assemblyPath">Path to the assembly to be weaved</param>
        /// <param name="configuration">An instance of <see cref="TraceLoggingConfiguration"/>.</param>
        public static void Execute(string assemblyPath, TraceLoggingConfiguration configuration)
        {
            ModuleDefinition moduleDef = null;

            var pdbFile = Path.ChangeExtension(assemblyPath, "pdb");
            var hasPdb = File.Exists(pdbFile);

            if (hasPdb)
            {
                using (var symbolStream = File.OpenRead(pdbFile))
                {
                    var readerParameters = new ReaderParameters
                    {
                        AssemblyResolver = new DefaultAssemblyResolver(),
                        ReadSymbols = true,
                        SymbolReaderProvider = new PdbReaderProvider(),
                        SymbolStream = symbolStream
                    };

                    moduleDef = ModuleDefinition.ReadModule(assemblyPath, readerParameters);
                }
            }
            else
            {
                moduleDef = ModuleDefinition.ReadModule(assemblyPath);
            }

            //execute weaving
            TraceLoggingWeaver.Execute(configuration, moduleDef);

            //write back the results
            var writerParameters = new WriterParameters
            {
                WriteSymbols = hasPdb,
                SymbolWriterProvider = new PdbWriterProvider()
            };

            moduleDef.Write(assemblyPath, writerParameters);
        }
    }
}
