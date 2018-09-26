using CanalSharp.Common.Logging;
using Microsoft.Extensions.Logging;

namespace CanalSharp.Common.Alarm.Impl
{
    public class LogAlarmHandler : ICanalAlarmHandler
    {
        private readonly ILogger _logger = CanalSharpLogManager.LoggerFactory.CreateLogger<LogAlarmHandler>();

        public void SendAlarm(string destination, string msg)
        {
            _logger.LogWarning($"destination:{destination}[{msg}]");
        }
    }
}