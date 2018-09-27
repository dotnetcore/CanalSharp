using System;
using Microsoft.Extensions.Logging;

namespace CanalSharp.Common.Logging
{
    public static class CanalSharpLogManager
    {
        public static readonly ILoggerFactory LoggerFactory = new LoggerFactory();
    }
}