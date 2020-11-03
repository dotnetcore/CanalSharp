using System;

namespace CanalSharp.Connections
{
    /// <summary>
    /// Connection state with Canal Server
    /// </summary>
    [Flags]
    public enum ConnectionState
    {
        Closed = 2 << 0,
        Connected = 2 << 1,
        Subscribed = 2 << 2,
        Unsubscribed = 2 << 3,
        Unsubscribing = 2 << 4
    }
}