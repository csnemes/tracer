using System;
using System.Globalization;
using System.Threading;
using Tracer.Serilog;
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
            Log.Verbose("message");
            Log.Verbose("Logging integer {intVal} and string {stringVal}", 42, "hello");
            Log.Verbose("StructLog {@destr}", new { StringVal = "hello", IntVal = 42 });
            Log.Verbose(new ApplicationException("error"), "message");
            Log.Verbose(new ApplicationException("error"), "Logging integer {intVal}and string {stringVal}", 42, "hello");

            Log.Debug("message");
            Log.Debug("Logging integer {intVal} and string {stringVal}", 42, "hello");
            Log.Debug("StructLog {@destr}", new { StringVal = "hello", IntVal = 42 });
            Log.Debug(new ApplicationException("error"), "message");
            Log.Debug(new ApplicationException("error"), "Logging integer {intVal}and string {stringVal}", 42, "hello");

            Log.Information("message");
            Log.Information("Logging integer {intVal} and string {stringVal}", 42, "hello");
            Log.Information("StructLog {@destr}", new { StringVal = "hello", IntVal = 42 });
            Log.Information(new ApplicationException("error"), "message");
            Log.Information(new ApplicationException("error"), "Logging integer {intVal}and string {stringVal}", 42, "hello");

            Log.Warning("message");
            Log.Warning("Logging integer {intVal} and string {stringVal}", 42, "hello");
            Log.Warning("StructLog {@destr}", new { StringVal = "hello", IntVal = 42 });
            Log.Warning(new ApplicationException("error"), "message");
            Log.Warning(new ApplicationException("error"), "Logging integer {intVal}and string {stringVal}", 42, "hello");

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
