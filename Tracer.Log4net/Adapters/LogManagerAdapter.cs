using System;

namespace Tracer.Log4Net.Adapters
{
    public static class LogManagerAdapter
    {
        public static LoggerAdapter GetLogger(Type type)
        {
            return new LoggerAdapter(type);
        }
    }
}
