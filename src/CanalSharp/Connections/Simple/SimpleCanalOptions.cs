namespace CanalSharp.Connections
{
    public class SimpleCanalOptions: CanalOptionsBase
    {
        public SimpleCanalOptions(string host, int port, string clientId)
        {
            Host = host;
            Port = port;
            ClientId = clientId;
        }
        /// <summary>
        /// Canal Server Host(Require)
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// Canal Server Port(Require)
        /// </summary>
        public int Port { get; set; }

    }
}