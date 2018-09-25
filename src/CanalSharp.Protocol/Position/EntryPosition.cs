using System;

namespace CanalSharp.Protocol.Position
{
    public class EntryPosition:TimePosition
    {
        public static  int EVENTIDENTITY_SEGMENT = 3;
        public static char EVENTIDENTITY_SPLIT = (char)5;

        public bool Included => false;

        public string JournalName { get; set; }
        public long? Position { get; set; }

        /// <summary>
        /// 记录一下位点对应的serverId
        /// </summary>
        public long? ServerId { get; set; }

        public string Gtid = null;
        

        public EntryPosition() : base(null)
        {

        }

        public EntryPosition(long? timestamp):this(null,null,timestamp)
        {
        }

        public EntryPosition(string journalName, long? position):this(journalName,position,null)
        {
        }

        public EntryPosition(string journalName, long? position, long? timestamp):base(timestamp)
        {
            JournalName = journalName;
            Position = position;
        }

        public EntryPosition(string journalName, long position, long timestamp, long? serverId):this(journalName, position, timestamp)
        {
            ServerId = serverId;
        }

        public override int GetHashCode()
        {
            const int prime = 31;
            var result = base.GetHashCode();
            result = prime * result + ((JournalName == null) ? 0 : JournalName.GetHashCode());
            result = prime * result + ((Position == null) ? 0 : Position.GetHashCode());
            // 手写equals，自动生成时需注意
            result = prime * result + ((Timestamp == null) ? 0 : Timestamp.GetHashCode());
            return result;
        }

        public override bool Equals(object obj)
        {
            if (this == obj)
            {
                return true;
            }
            if (!base.Equals(obj))
            {
                return false;
            }
            if (!(obj is EntryPosition)) {
                return false;
            }
            var other = (EntryPosition)obj;
            if (JournalName == null)
            {
                if (other.JournalName != null)
                {
                    return false;
                }
            }
            else if (!JournalName.Equals(other.JournalName))
            {
                return false;
            }
            if (Position == null)
            {
                if (other.Position != null)
                {
                    return false;
                }
            }
            else if (!Position.Equals(other.Position))
            {
                return false;
            }
            // 手写equals，自动生成时需注意
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


        public int CompareTo(EntryPosition o)
        {
            var val = string.Compare(JournalName, o.JournalName, StringComparison.Ordinal);

            if (val != 0) return val;
            if (Position == null) return val;
            if (o.Position != null) return (int) (Position - o.Position);
            return val;
        }
    }
}
