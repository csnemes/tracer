using System;
using System.Globalization;
using System.Threading;
using Tracer.NLog;
using TracerAttributes;

namespace TestApplication.Serilog.Netstd
{
    public class MyNetstandardClass
    {
        public int AddTwoNumbers(int num1, int num2)
        {
            InternalMethod("Nothing");
            StaticLogRewrites();
            GenericMethodTests();
            return num1 + num2;
        }

        private void InternalMethod(string param)
        {
            Thread.Sleep(100);
        }

        public void StaticLogRewrites()
        {
            Log.Debug("message");
            Log.Debug("Logging integer {intVal} and string {stringVal}", 42, "hello");
            Log.Debug("StructLog {@destr}", new { StringVal = "hello", IntVal = 42 });
            Log.Debug(new ApplicationException("error"), "message");
            Log.Debug(new ApplicationException("error"), "Logging integer {intVal}and string {stringVal}", 42, "hello");

            Log.Info("message");
            Log.Info("Logging integer {intVal} and string {stringVal}", 42, "hello");
            Log.Info("StructLog {@destr}", new { StringVal = "hello", IntVal = 42 });
            Log.Info(new ApplicationException("error"), "message");
            Log.Info(new ApplicationException("error"), "Logging integer {intVal}and string {stringVal}", 42, "hello");

            Log.Warn("message");
            Log.Warn("Logging integer {intVal} and string {stringVal}", 42, "hello");
            Log.Warn("StructLog {@destr}", new { StringVal = "hello", IntVal = 42 });
            Log.Warn(new ApplicationException("error"), "message");
            Log.Warn(new ApplicationException("error"), "Logging integer {intVal}and string {stringVal}", 42, "hello");

            Log.Error("message");
            Log.Error("Logging integer {intVal} and string {stringVal}", 42, "hello");
            Log.Error("StructLog {@destr}", new { StringVal = "hello", IntVal = 42 });
            Log.Error(new ApplicationException("error"), "message");
            Log.Error(new ApplicationException("error"), "Logging integer {intVal}and string {stringVal}", 42, "hello");

            Log.Fatal("message");
            Log.Fatal("Logging integer {intVal} and string {stringVal}", 42, "hello");
            Log.Fatal("StructLog {@destr}", new { StringVal = "hello", IntVal = 42 });
            Log.Fatal(new ApplicationException("error"), "message");
            Log.Fatal(new ApplicationException("error"), "Logging integer {intVal}and string {stringVal}", 42, "hello");
            //closure
            int idx = 1;
            Action act = () =>
            {
                idx++;
                Log.Debug("Closure log");
            };

            act();

            //lambda w/out closure
            Action act2 = () =>
            {
                Log.Debug("No closure log");
            };

            act2();
        }

        [TraceOn]
        private void GenericMethodTests()
        {
            Write(42);
            Write("Hello");
            Write(0.5);
        }

        public void Write<T>(T input)
        {
            Console.WriteLine(input);
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
