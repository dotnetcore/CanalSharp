using System;

namespace Canal.Csharp.Abstract.Logging
{
    internal class NullLogger : ILogger
    {
        public void Debug(String message)
        {
        }

        public void Info(String message)
        {
        }

        public void Warning(String message)
        {
        }

        public void Error(String message, Exception exception)
        {
        }

        public void Trace(String message)
        {
        }
    }
}