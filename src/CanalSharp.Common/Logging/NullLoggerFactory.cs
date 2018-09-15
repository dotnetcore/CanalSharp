using System;

namespace Canal.Csharp.Abstract.Logging
{
    public class NullLoggerFactory : ILoggerFactory
    {
        public ILogger CreateLogger(Type type)
        {
            return new NullLogger();
        }
    }
}