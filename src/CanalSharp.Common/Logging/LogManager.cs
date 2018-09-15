using System;
using Servicecomb.Saga.Omega.Abstractions.Logging;

namespace Canal.Csharp.Abstract.Logging
{
    public static class LogManager
    {
        private static readonly ILoggerFactory defaultLoggerFactory = new NullLoggerFactory();
        private static ILoggerFactory _loggerFactory;

        public static ILogger GetLogger(Type type)
        {
            var loggerFactory = _loggerFactory ?? defaultLoggerFactory;
            return loggerFactory.CreateLogger(type);
        }

        public static ILogger GetLogger<T>()
        {
            return GetLogger(typeof(T));
        }

        public static void SetLoggerFactory(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }
    }
}