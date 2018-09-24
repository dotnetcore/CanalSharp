namespace CanalSharp.Client.Impl.Running
{
    /// <summary>
    /// client running状态信息
    /// </summary>
    public class ClientRunningData
    {
        private short ClientId { get; set; }
        public string Address { get; }
        public bool Active { get; } = true;

        public bool IsActive()
        {
            return Active;
        }
    }
}