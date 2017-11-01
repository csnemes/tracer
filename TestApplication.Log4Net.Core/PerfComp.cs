using System;
using System.Diagnostics;
using log4net;
using Tracer.Log4Net;

namespace TestApplication.Log4Net.Core
{
    public class PerfComp
    {
        private readonly static ILog _log4net = LogManager.GetLogger(typeof(PerfComp));
        private const int LoopCnt = 10000;

        public void SpeedTest()
        {
            var sw = Stopwatch.StartNew();

            for (int i = 0; i < LoopCnt; i++)
            {
                Log.Debug("Write a string");
                Log.Debug("Write an exception", new ApplicationException());
            }

            sw.Stop();
            Console.WriteLine("Tracer:{0} ms", sw.ElapsedMilliseconds);

            sw.Restart();
            for (int i = 0; i < LoopCnt; i++)
            {
                _log4net.Debug("Write a string");
                _log4net.Debug("Write an exception", new ApplicationException());
            }

            sw.Stop();
            Console.WriteLine("Log4Net:{0} ms", sw.ElapsedMilliseconds);
        }
    }
}
