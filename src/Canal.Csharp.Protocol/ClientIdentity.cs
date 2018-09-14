using System;
using System.Collections.Generic;
using System.Text;

namespace Canal.Csharp.Protocol
{
    [Serializable]
    public class ClientIdentity
    {
        public  string Destination { get; set; }
        public  short ClientId { get; set; }
        public  string Filter { get; set; }

        public ClientIdentity()
        {

        }

        public ClientIdentity(string destination,short clientId)
        {
            Destination = destination;
            ClientId = clientId;
        }
        public ClientIdentity(string destination, short clientId,string filter)
        {
            Destination = destination;
            ClientId = clientId;
            Filter = filter;
        }

        public bool HasFilter()
        {
            return Filter != null && string.IsNullOrEmpty(Filter);
        }

        public override int GetHashCode()
        {
            const int prime = 31;
            var result = 1;
            result = prime * result + ClientId;
            result = prime * result + ((Destination == null) ? 0 : Destination.GetHashCode());
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
            if (!(obj is ClientIdentity)) {
                return false;
            }
            var other = (ClientIdentity)obj;
            if (ClientId != other.ClientId)
            {
                return false;
            }
            if (Destination == null)
            {
                if (other.Destination != null)
                {
                    return false;
                }
            }
            else if (!Destination.Equals(other.Destination))
            {
                return false;
            }
            return true;
        }
    }
}
