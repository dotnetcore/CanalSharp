namespace CanalSharp.Protocol.Exception
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
