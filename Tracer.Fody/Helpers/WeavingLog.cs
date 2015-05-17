using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tracer.Fody.Helpers
{
    internal static class WeavingLog
    {
        private static IWeavingLogger _logger = new NullLogger();

        public static void SetLogger(IWeavingLogger logger)
        {
            _logger = logger ?? new NullLogger();
        }

        public static void LogDebug(string message)
        {
            _logger.LogDebug(message);
        }

        public static void LogInfo(string message)
        {
            _logger.LogInfo(message);
        }

        public static void LogWarning(string message)
        {
            _logger.LogWarning(message);
        }

        public static void LogError(string message)
        {
            _logger.LogError(message);
        }


        private class NullLogger : IWeavingLogger
        {
            void IWeavingLogger.LogDebug(string message)
            {}

            void IWeavingLogger.LogInfo(string message)
            {}

            void IWeavingLogger.LogWarning(string message)
            {}

            void IWeavingLogger.LogError(string message)
            {}
        }
    }
}
