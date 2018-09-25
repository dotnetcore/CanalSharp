using System;
using CanalSharp.Common.Logging;
using NLog;

namespace CanalSharp.Logging.NLog
{
    public class NLogLoggerFactory:ICanalSharpLoggerFactory
    {
        public ICanalSharpLogger CreateLogger(Type type)
        {
            return new NLogLogger(LogManager.GetLogger("CanalSharp",type));
        }

        internal class NLogLogger:ICanalSharpLogger
        {
            private readonly Logger _logger;

            public NLogLogger(Logger logger)
            {
                _logger = logger;
            }
            public void Debug(string message)
            {
                _logger.Debug(message);
            }

            public void Info(string message)
            {
                _logger.Info(message);
            }

            public void Warning(string message)
            {
                _logger.Warn(message);
            }

            public void Error(string message, Exception exception)
            {
                _logger.Error(message,exception,null);
            }

            public void Trace(string message)
            {
                _logger.Trace(message);
            }
        }
    }
}
