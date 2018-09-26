using System;

namespace CanalSharp.Common.Logging
{
    public static class CanalSharpLogManager
    {
        private static readonly ICanalSharpLoggerFactory DefaultLoggerFactory = new CanalSharpNullLoggerFactory();
        private static ICanalSharpLoggerFactory _loggerFactory;

        public static ICanalSharpLogger GetLogger(Type type)
        {
            var loggerFactory = _loggerFactory ?? DefaultLoggerFactory;
            return loggerFactory.CreateLogger(type);
        }

        public static ICanalSharpLogger GetLogger<T>()
        {
            return GetLogger(typeof(T));
        }

        public static void SetLoggerFactory(ICanalSharpLoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }
    }
}