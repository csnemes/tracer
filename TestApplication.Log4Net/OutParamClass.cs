using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestApplication
{
    public class OutParamClass
    {
        public string SetParamString(string input, out string mypara)
        {
            mypara = input;
            return String.Concat(input, input);
        }

        public void SetParamInt(string input, out int mypara)
        {
            mypara = Int32.Parse(input);
        }
    }
}
