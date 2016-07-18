using System;
using System.Diagnostics;
using System.Text;

namespace Tracer.OutputWindow.Adapters
{
    public class LoggerAdapter
    {
        private const string NullString = "<NULL>";
        private readonly Type _type;

        public LoggerAdapter(Type type)
        {
            _type = type;
        }

        #region Methods required for trace enter and leave

        public void TraceEnter(string methodInfo, string[] paramNames, object[] paramValues)
        {
            string message;

            if (paramNames != null)
            {
                StringBuilder parameters = new StringBuilder();
                for (int i = 0; i < paramNames.Length; i++)
                {
                    parameters.AppendFormat("{0}={1}", paramNames[i], paramValues[i] ?? NullString);
                    if (i < paramNames.Length - 1) parameters.Append(", ");
                }
                string argInfo = parameters.ToString();
                message = String.Format("Entered into {0} ({1}).", methodInfo, argInfo);
            }
            else
            {
                message = String.Format("Entered into {0}.", methodInfo);
            }

            Debug.WriteLine(message);
        }

        public void TraceLeave(string methodInfo, long startTicks, long endTicks, string[] paramNames,
            object[] paramValues)
        {
            string returnValue = null;

            if (paramNames != null)
            {
                StringBuilder parameters = new StringBuilder();
                for (int i = 0; i < paramNames.Length; i++)
                {
                    parameters.AppendFormat("{0}={1}", paramNames[i] ?? "$return", paramValues[i] ?? NullString);
                    if (i < paramNames.Length - 1) parameters.Append(", ");
                }
                returnValue = parameters.ToString();
            }

            double timeTaken = ConvertTicksToMilliseconds(endTicks - startTicks);

            string message = String.Format("Returned from {1} ({2}). Time taken: {0:0.00} ms.", timeTaken, methodInfo,
                returnValue);

            Debug.WriteLine(message);
        }

        #endregion

        private static double ConvertTicksToMilliseconds(long ticks)
        {
            //ticks * tickFrequency * 10000
            return ticks*(10000000/(double) Stopwatch.Frequency)/10000L;
        }
    }
}