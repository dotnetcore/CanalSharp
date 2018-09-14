using System;
using System.Collections.Generic;
using System.Text;

namespace Canal.Csharp.Protocol.Exception
{
    public class CanalClientException : System.Exception
    {
        public CanalClientException(string errorCode) : base(errorCode)
        {
        }

        public CanalClientException(string errorCode, System.Exception cause) : base(errorCode, cause)
        {
        }
    }
}
