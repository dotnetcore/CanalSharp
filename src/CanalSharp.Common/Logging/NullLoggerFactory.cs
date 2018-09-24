using System;

namespace CanalSharp.Common.Logging
{
    public class NullLoggerFactory : ILoggerFactory
    {
        public ILogger CreateLogger(Type type)
        {
            return new NullLogger();
        }
    }
}