using System;

namespace CanalSharp.Common.Logging
{
    public interface ILoggerFactory
    {
        ILogger CreateLogger(Type type);
    }
}