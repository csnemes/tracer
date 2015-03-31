using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tracer.Log4net.Adapters;

namespace TestApplication
{
    public class NoRewriteGenericClass<T>
    {

        private static LoggerAdapter _loggerAdapter = LogManagerAdapter.GetLogger(typeof (NoRewriteGenericClass<>));
    }
}
