using System;
using CanalSharp.Common.Logging;
using Microsoft.Extensions.Logging;

namespace CanalSharp.Logging.Microsoft.Extensions.Logging
{
    public class MicrosoftExtensionsLoggingLoggerFactory:ICanalSharpLoggerFactory
    {
        private readonly ILoggerFactory _loggerFactory;

        public MicrosoftExtensionsLoggingLoggerFactory(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }
        public ICanalSharpLogger CreateLogger(Type type)
        {
            return new MicrosoftExtensionsLoggingLogger(_loggerFactory.CreateLogger(type));
        }

        internal class MicrosoftExtensionsLoggingLogger:ICanalSharpLogger
        {
            private readonly ILogger _logger;

            public MicrosoftExtensionsLoggingLogger(ILogger logger)
            {
                _logger = logger;
            }
            public void Debug(string message)
            {
                _logger.LogDebug(message);
            }

            public void Info(string message)
            {
                _logger.LogInformation(message);
            }

            public void Warning(string message)
            {
                _logger.LogWarning(message);
            }

            public void Error(string message, Exception exception)
            {
                _logger.LogError(exception,message);
            }

            public void Trace(string message)
            {
                _logger.LogTrace(message);
            }
        }
    }
}
