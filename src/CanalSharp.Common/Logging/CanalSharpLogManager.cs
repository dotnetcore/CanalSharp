using System;

namespace CanalSharp.Common.Logging
{
    public static class CanalSharpLogManager
    {
        private static readonly ILoggerFactory DefaultLoggerFactory = new NullLoggerFactory();
        private static ILoggerFactory _loggerFactory;

        public static ILogger GetLogger(Type type)
        {
            var loggerFactory = _loggerFactory ?? DefaultLoggerFactory;
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