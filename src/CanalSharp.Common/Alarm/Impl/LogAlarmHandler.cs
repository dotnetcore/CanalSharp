using CanalSharp.Common.Logging;

namespace CanalSharp.Common.Alarm.Impl
{
    public class LogAlarmHandler : ICanalAlarmHandler
    {
        private readonly ICanalSharpLogger _logger = CanalSharpLogManager.GetLogger(typeof(LogAlarmHandler));
        public void SendAlarm(string destination, string msg)
        {
            _logger.Warning($"destination:{destination}[{msg}]");
        }
    }
}
