namespace CanalSharp.Common.Exception
{
    public class CanalException : System.Exception
    {
        public CanalException(string errorCode) : base(errorCode)
        {
        }

        public CanalException(string errorCode, System.Exception cause) : base(errorCode, cause)
        {
        }
    }
}
