using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog.Events;
using Tracer.Serilog;

namespace TestApplication.Serilog
{
    public class MyApplication
    {
        public void Run()
        {

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

            OutParamLogs();
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
    }
}
