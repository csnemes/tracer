using System;
using System.Linq;
using Serilog.Events;
using Tracer.Serilog;

namespace TestApplication.Serilog
{
    public class MyApplication
    {
        public void Run()
        {
            InlineTest(42);

            Log.Verbose("message");
            Log.Verbose("Logging integer {intVal} and string {stringVal}", 42, "hello");
            Log.Verbose("StructLog {@destr}", new { StringVal = "hello", IntVal = 42 });
            Log.Verbose(new ApplicationException("error"), "message");
            Log.Verbose(new ApplicationException("error"), "Logging integer {intVal}and string {stringVal}", 42, "hello");

            Log.Debug("message");
            Log.Debug("Logging integer {intVal} and string {stringVal}", 42, "hello");
            Log.Debug("StructLog {@destr}", new { StringVal="hello", IntVal = 42});
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

            if (Log.IsEnabled(LogEventLevel.Debug))
            {
                Log.Debug("hello");                
            }

            Log.Write(LogEventLevel.Debug, "message");
            Log.Write(LogEventLevel.Debug, "Logging integer {intVal}and string {stringVal}", 42, "hello");
            Log.Write(LogEventLevel.Debug, new ApplicationException("error"), "message");
            Log.Write(LogEventLevel.Debug, new ApplicationException("error"), "Logging integer {intVal}and string {stringVal}", 42, "hello");

            var result = DoSomething(42, "John");

            var sealedClass = new MySealedClass();
            sealedClass.DoubleThat(2);

            OutParamLogs();

            ReceivingStructures(new MyClass());

            MyClass mClass;
            ReturningStructures("input", out mClass);

            ReturningStructures2("input");

            NullInOut(null);

            try {
                ThrowException();
            }
            catch {}

            var g1 = new GenericClass<string>();
            g1.DoNothing("Hello");

            var g2 = new GenericClass<int>();
            g2.DoNothing(42);
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

        public string ThrowException()
        {
            throw new NotImplementedException();
        }

        public string DoSomething(int inp, string name)
        {
            return "Hi " + name + '!';
        }

        public void OutParamLogs()
        {
            var op = new OutParamClass();
            string outString;
            op.SetParamString("in", out outString);
            int outInt;
            op.SetParamInt("42", out outInt);
        }

        public MyClass ReturningStructures(string input, out MyClass myClass)
        {
            myClass = new MyClass();
            return new MyClass();
        }

        public SomeOtherClass ReturningStructures2(string input)
        {
            return new SomeOtherClass() {StringValue = "hello" };
        }

        public void ReceivingStructures(MyClass myClass)
        {
            var x = myClass.StringValue;
        }

        public string NullInOut(string inp)
        {
            return null;
        }
    }

    public sealed class MySealedClass
    {
        public MySealedClass()
        {}

        public int DoubleThat(int inp)
        {
            return inp * 2;
        }
    }

    [Destructure]
    public class MyClass
    {
        public MyClass()
        {
            StringValue = "LoremIpsum";
            NumericValue = 42;
            InnerClass = new MyInnerClass()
            {
                StringValue = "Inner",
                NumericValue = 43
            };
        }

        public string StringValue { get; set; }
        public int NumericValue { get; set; }
        public MyInnerClass InnerClass { get; set; }
    }

    public class MyInnerClass
    {
        public string StringValue { get; set; }
        public int NumericValue { get; set; }
    }

    public class SomeOtherClass
    {
        public string StringValue { get; set; }
    }
}
