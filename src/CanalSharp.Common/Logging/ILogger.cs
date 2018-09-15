using System;

namespace CanalSharp.Common.Logging
{
    public interface ILogger
    {
        void Debug(String message);

        void Info(String message);

        void Warning(String message);

        void Error(String message, System.Exception exception);

        void Trace(String message);
    }
}