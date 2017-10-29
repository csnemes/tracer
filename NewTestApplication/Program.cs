using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using log4net.Config;
using Tracer.Log4Net;
using Tracer.Log4Net.Adapters;

namespace NewTestApplication
{
    class Program
    {
        private static LoggerAdapter _lap = new LoggerAdapter(typeof(Program));
        static void Main(string[] args)
        {
            BasicConfigurator.Configure();
            _lap.LogDebug("methodInfo", "message");
            Log.Debug("Hello");
            Console.ReadLine();
        }
    }
}
