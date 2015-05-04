using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tracer.Fody.Tests.MockLoggers
{
    public static class MockLog
    {
        //When rewriting calls to this method they will be redirected to the adapter's MockLogOuter method.
        //All adapter methods will have and additional string methodInfo first parameter so the corresponding method will be
        //public void MockLogOuter(string methodInfo string message)
        public static void Outer(string message)
        {}

        public static void OuterNoParam()
        {}

        public static void Outer(string message, int i)
        {}
    }
}
