
namespace CanalSharp.Common.Logging
{
    public interface ILogger
    {
        void Debug(string message);

        void Info(string message);

        void Warning(string message);

        void Error(string message, System.Exception exception);

        void Trace(string message);
    }
}