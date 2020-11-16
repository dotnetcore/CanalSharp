﻿namespace CanalSharp.Connections
{
    public class ClusterCanalConnectionOptions : CanalConnectionOptionsBase
    {
        public ClusterCanalConnectionOptions(string zkAddress, string clientId)
        {
            ClientId = clientId;
            ZkAddress = zkAddress;
        }

        public string ZkAddress { get; set; }
        public int ZkSessionTimeout { get; set; } = 5000;
    }
}