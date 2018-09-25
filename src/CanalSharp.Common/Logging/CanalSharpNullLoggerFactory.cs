using System;

namespace CanalSharp.Common.Logging
{
    public class CanalSharpNullLoggerFactory : ICanalSharpLoggerFactory
    {
        public ICanalSharpLogger CreateLogger(Type type)
        {
            return new CanalSharpNullLogger();
        }
    }
}