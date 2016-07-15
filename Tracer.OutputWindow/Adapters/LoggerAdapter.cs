using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace Tracer.OutputWindow.Adapters
{
    public class LoggerAdapter
    {
        private const string NullString = "<NULL>";
        private readonly Type _type;
        private readonly string _typeNamespace;

        public LoggerAdapter(Type type)
        {
            _type = type;
            _typeNamespace = type.Namespace;
        }

        #region Methods required for trace enter and leave

        public void TraceEnter(string methodInfo, string[] paramNames, object[] paramValues)
        {

            string message;

            if (paramNames != null)
            {
                var parameters = new StringBuilder();
                for (int i = 0; i < paramNames.Length; i++)
                {
                    parameters.AppendFormat("{0}={1}", paramNames[i], paramValues[i] ?? "<NULL>");
                    if (i < paramNames.Length - 1) parameters.Append(", ");
                }
                var argInfo = parameters.ToString();
                message = String.Format("Entered into {0} ({1}).", methodInfo, argInfo);
            }
            else
            {
                message = String.Format("Entered into {0}.", methodInfo);
            }

            Debug.WriteLine(message);

        }

        public void TraceLeave(string methodInfo, long startTicks, long endTicks, string[] paramNames, object[] paramValues)
        {

            string returnValue = null;

            if (paramNames != null)
            {
                var parameters = new StringBuilder();
                for (int i = 0; i < paramNames.Length; i++)
                {
                    parameters.AppendFormat("{0}={1}", paramNames[i] ?? "$return", paramValues[i] ?? "<NULL>");
                    if (i < paramNames.Length - 1) parameters.Append(", ");
                }
                returnValue = parameters.ToString();
            }

            var timeTaken = ConvertTicksToMilliseconds(endTicks - startTicks);

            string message = String.Format("Returned from {1} ({2}). Time taken: {0:0.00} ms.", timeTaken, methodInfo, returnValue);

            Debug.WriteLine(message);

        }

        #endregion

        private static double ConvertTicksToMilliseconds(long ticks)
        {
            //ticks * tickFrequency * 10000
            return ticks * (10000000 / (double)Stopwatch.Frequency) / 10000L;
        }


    }
}
