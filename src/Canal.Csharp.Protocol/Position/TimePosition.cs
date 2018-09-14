namespace CanalSharp.Protocol.Position
{
    public class TimePosition: Position
    {
        protected long? Timestamp { get; set; }

        public TimePosition(long? timestamp)
        {
            Timestamp = timestamp;
        }

        public override int GetHashCode()
        {
            const int prime = 31;
            var result = 1;
            result = prime * result + ((Timestamp == null) ? 0 : Timestamp.GetHashCode());
            return result;
        }

        public override bool Equals(object obj)
        {
            if (this == obj)
            {
                return true;
            }
            if (obj == null)
            {
                return false;
            }
            if (!(obj is TimePosition)) {
                return false;
            }
            var other = (TimePosition)obj;
            if (Timestamp == null)
            {
                if (other.Timestamp != null)
                {
                    return false;
                }
            }
            else if (!Timestamp.Equals(other.Timestamp))
            {
                return false;
            }
            return true;
        }
    }
}
