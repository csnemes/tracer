using System;
using Serilog;
using TestApplication.Serilog.Netstd;

namespace TestApplication.Serilog.Core
{
    class Program
    {
        static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.RollingFile("c:\\ApplicationLogs\\log-{Date}.txt",
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{MachineName}][{ThreadId}][{Level}]{ClassName}.{MethodName} {Message}{NewLine}{Exception}")
                .Enrich.WithMachineName()
                .Enrich.WithThreadId()
                .CreateLogger();
            Console.WriteLine("Starting application...");

            var myApp = new MyApplication();
            myApp.Run();

            var mnc = new MyNetstandardClass();
            mnc.AddTwoNumbers(20, 22);

            Console.WriteLine("Press Enter to stop");
            Console.ReadLine();
        }
    }
}
