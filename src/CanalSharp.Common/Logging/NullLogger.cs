using System;

namespace CanalSharp.Common.Logging
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

        public void Error(String message, System.Exception exception)
        {
        }

        public void Trace(String message)
        {
        }
    }
}