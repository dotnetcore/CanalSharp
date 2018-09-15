using System;
using System.Collections.Generic;
using System.Text;

namespace CanalSharp.Common.Alarm.Impl
{
    public class LogAlarmHandler : ICanalAlarmHandler
    {
        private readonly ILogger _logger = LogManager.GetLogger(typeof(LogAlarmHandler));
        public void SendAlarm(string destination, string msg)
        {
            _logger.Warning($"destination:{destination}[{msg}]");
        }
    }
}
