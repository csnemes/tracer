using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tracer.Log4Net;

namespace TestApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure(new FileInfo("Logging.config"));
            Console.WriteLine("Starting application...");
            var myApp = new MyApplication();
            myApp.Run();
            Console.WriteLine("Press Enter to stop");
            Console.ReadLine();
        }
    }
}
