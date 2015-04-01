using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tracer.Log4net;

namespace TestApplication
{
    public class MyApplication
    {
        public void Run()
        {
            Thread.Sleep(500);
            Write(42);
            Write("Hello");

            Log.InfoEvent((object)"Faszikám");

            Write(Add(21, 22));

            var intGen = new GenericClass<int>();
            Write(intGen.GetDefault(42));
        }

        public void Write<T>(T input)
        {
            Console.WriteLine(input);
        }

        public int Add(int i1, int i2)
        {
            return i1 + i2;
        }
    }
}
