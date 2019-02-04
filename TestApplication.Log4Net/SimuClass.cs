using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TracerAttributes;

namespace TestApplication
{
    [NoTrace]
    public class SimuClassBase
    {
        public SimuClassBase(string input)
        {
        }
    }

    [NoTrace]
    public class SimuClass : SimuClassBase
    {
        private string test = "aa";

        public SimuClass(string inp):base(inp)
        {
            try
            {
                var inp2 = inp + "a";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }
    }
}
