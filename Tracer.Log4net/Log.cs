using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tracer.Log4net
{
    public static class Log
    {
        public static void Error(Exception ex)
        {}

        public static void Error(string message, Exception ex)
        {}
    }
}
