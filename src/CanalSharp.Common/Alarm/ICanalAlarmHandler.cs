namespace CanalSharp.Common.Alarm
{
    /// <summary>
    ///  canal报警处理机制
    /// </summary>
    public interface ICanalAlarmHandler
    {
        /// <summary>
        /// 发送对应destination的报警
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="msg"></param>
        void SendAlarm(string destination, string msg);
    }
}