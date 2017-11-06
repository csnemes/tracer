using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestApplication
{
    public class StructParamClass
    {
        public struct MyStruct
        {
            public int IntVal { get; set; }
        }

        public void RunStructs()
        {
            var x = StructReturn();
            StructIn(x);
            StructOut(out x);
        }

        public MyStruct StructReturn()
        {
            return new MyStruct() {IntVal = 1};
        }
        public void StructIn(MyStruct inp)
        {
        }

        public void StructOut(out MyStruct inp)
        {
            inp = new MyStruct() { IntVal = 1 };
        }

    }
}
