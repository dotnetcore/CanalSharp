using System;

namespace Canal.Csharp.Abstract.Logging
{
    public interface ILoggerFactory
    {
        ILogger CreateLogger(Type type);
    }
}