namespace CanalSharp.Protocol.Position
{
    public class MetaqPosition : Position
    {
        public string Topic { get; set; }

        public string MsgNewId { get; set; }

        public long Offset { get; set; }

        public MetaqPosition(string topic, string msgNewId, long offset)
        {
            Topic = topic;
            MsgNewId = msgNewId;
            Offset = offset;
        }
    }
}