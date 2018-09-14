using System;
using System.Collections.Generic;
using System.Text;

namespace Canal.Csharp.Protocol.Position
{
    public class LogPosition: Position
    {
        public LogIdentity Identity { get; set; }
        private EntryPosition Postion { get; set; }

        public override int GetHashCode()
        {
            const int prime = 31;
            var result = 1;
            result = prime * result + ((Identity == null) ? 0 : Identity.GetHashCode());
            result = prime * result + ((Postion == null) ? 0 : Postion.GetHashCode());
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
            if (!(obj is LogPosition)) {
                return false;
            }
            var other = (LogPosition)obj;
            if (Identity == null)
            {
                if (other.Identity != null)
                {
                    return false;
                }
            }
            else if (!Identity.Equals(other.Identity))
            {
                return false;
            }
            if (Postion == null)
            {
                if (other.Postion != null)
                {
                    return false;
                }
            }
            else if (!Postion.Equals(other.Postion))
            {
                return false;
            }
            return true;
        }

        public boolean equals(Object obj)
        {
           
        }
    }
}
