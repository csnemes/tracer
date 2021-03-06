﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;
using Serilog.Events;

namespace TestApplication.PatternFilter
{
    public class Program
    {
        public static void Main(string[] args)
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
            Console.WriteLine("Press Enter to stop");
            Console.ReadLine();
        }
    }
}
