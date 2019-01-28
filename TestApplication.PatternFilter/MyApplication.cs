using System;
using System.Linq;
using Tracer.Serilog;

namespace TestApplication.PatternFilter
{
    public class MyApplication
    {
        public void Run()
        {
            InlineTest(42);
            MyMethod();
            OtherMethod();
            PrivateMethod();
            var x = new MyOtherClass();
            x.MyMethod();
            x.OtherMethod();
        }

        public string InlineTest(int input1)
        {
            var locStr = "Hello2";
            var x = Inline(42, "Hello");
            return x.Reverse().ToString();

            string Inline(int inp, string inp2)
            {
                return inp2 + locStr + input1;
            }
        }

        public void MyMethod() { }

        public void OtherMethod() { }

        private void PrivateMethod() { }
    }

    class MyOtherClass
    {
        public void MyMethod() { }

        public void OtherMethod()
        {
            PrivateMethod();
        }

        private void PrivateMethod() { }
    }

}
