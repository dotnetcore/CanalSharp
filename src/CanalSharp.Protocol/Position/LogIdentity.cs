using System.Net;

namespace CanalSharp.Protocol.Position
{
    public class LogIdentity:Position
    {
        // 链接服务器的地址
        public SocketAddress SourceAddress { get; }                         
        // 对应的slaveId
        public long? SlaveId { get; }

        public LogIdentity()
        {

        }

        public LogIdentity(SocketAddress sourceAddress,long slaveId)
        {
            SourceAddress = sourceAddress;
            SlaveId = slaveId;
        }

        public override int GetHashCode()
        {
            const int prime = 31;
            var result = 1;
            result = prime * result + ((SlaveId == null) ? 0 : SlaveId.GetHashCode());
            result = prime * result + ((SourceAddress == null) ? 0 : SourceAddress.GetHashCode());
            return result;
        }

        public override bool Equals(object obj)
        {
            if (this == obj) return true;
            if (obj == null) return false;
            if (this != obj) return false;
            var other = (LogIdentity)obj;
            if (SlaveId == null)
            {
                if (other.SlaveId != null) return false;
            }
            else if (SlaveId != (other.SlaveId)) return false;
            if (SourceAddress == null)
            {
                if (other.SourceAddress != null) return false;
            }
            else if (!SourceAddress.Equals(other.SourceAddress)) return false;
            return true;
        }
    }
}
