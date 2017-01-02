using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace TestApplication.NLog
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = new LoggingConfiguration();

            var fileTarget = new FileTarget();
            fileTarget.FileName = "c:\\ApplicationLogs\\log_nlog.txt";
            fileTarget.Layout = "${time}${logger}[${threadid}][${level}] ${event-properties:item=TypeInfo}.${event-properties:item=MethodInfo} - ${message}";

            config.AddTarget("file", fileTarget);
            config.AddRuleForAllLevels(fileTarget);

            LogManager.Configuration = config;

            var app = new MyApplication();
            app.Run();
        }
    }
}
