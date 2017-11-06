using System;
using System.IO;
using System.Reflection;
using log4net;

namespace TestApplication.Log4Net.Core
{
    class Program
    {
        static void Main(string[] args)
        {
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            log4net.Config.XmlConfigurator.Configure(logRepository, new FileInfo("Logging.config"));
            Console.WriteLine("Starting application...");
            var myApp = new MyApplication();
            myApp.Run();
            Console.WriteLine("Press Enter to stop");
            Console.ReadLine();
        }
    }
}
