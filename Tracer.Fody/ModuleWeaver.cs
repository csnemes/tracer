using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;

namespace Tracer.Fody
{
    /// <summary>
    /// Class that links fody to the real weaver
    /// </summary>
    public class ModuleWeaver
    {
        public ModuleDefinition ModuleDefinition { get; set; }


        public void Execute()
        {

        }
    }
}
