using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using Tracer.Log4net;

namespace TestApplication
{
    public class MyApplication
    {
        public void Run()
        {
            Thread.Sleep(500);

            GenericMethodTests();
            GenericClassTests();

            StaticLogRewrites();

            OutParamLogs();

            Write(Add(21, 22));
            Write(Add(100, 1));

            var perfComp = new PerfComp();
            perfComp.SpeedTest();
        }

        public void OutParamLogs()
        {
            var op = new OutParamClass();
            string outString;
            op.SetParamString("in", out outString);
            int outInt;
            op.SetParamInt("42", out outInt);
        }

        public void StaticLogRewrites()
        {
            Log.Debug(new MyClass());
            Log.Debug("Hello");
            Log.Debug("Hello", new ApplicationException("Failure."));
            Log.DebugFormat("A{0}", 1);
            Log.DebugFormat("A{0}-B{1}", 1, 2);
            Log.DebugFormat("A{0}-B{1}-C{2}", 1, 2, 3);
            Log.DebugFormat("A{0}-B{1}-C{2}-D{3}", 1, 2, 3, 4);
            Log.DebugFormat(new CultureInfo("en-us"), "A{0}-B{1}-C{2}-D{3}", 1, 2, 3, 4);

            Log.Info(new MyClass());
            Log.Info("Hello");
            Log.Info("Hello", new ApplicationException("Failure."));
            Log.InfoFormat("A{0}", 1);
            Log.InfoFormat("A{0}-B{1}", 1, 2);
            Log.InfoFormat("A{0}-B{1}-C{2}", 1, 2, 3);
            Log.InfoFormat("A{0}-B{1}-C{2}-D{3}", 1, 2, 3, 4);
            Log.InfoFormat(new CultureInfo("en-us"), "A{0}-B{1}-C{2}-D{3}", 1, 2, 3, 4);
        }

        private void GenericMethodTests()
        {
            Write(42);
            Write("Hello");
            Write(0.5);
        }

        private void GenericClassTests()
        {
            var intGen = new GenericClass<int>();
            Write(intGen.GetDefault(42));

            var stringGen = new GenericClass<string>();
            Write(stringGen.GetDefault("Hello"));
        }

        public void Write<T>(T input)
        {
            Console.WriteLine(input);
        }

        public int Add(int i1, int i2)
        {
            return i1 + i2;
        }

        private class MyClass
        {
            public override string ToString()
            {
                return "ToStringOfMyClass";
            }
        }
    }
}
