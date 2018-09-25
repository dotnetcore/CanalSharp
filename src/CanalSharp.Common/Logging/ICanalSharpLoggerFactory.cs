using System;

namespace CanalSharp.Common.Logging
{
    public interface ICanalSharpLoggerFactory
    {
        ICanalSharpLogger CreateLogger(Type type);
    }
}