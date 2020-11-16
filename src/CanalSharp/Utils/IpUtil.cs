using System.Net;
using System.Net.Sockets;

namespace CanalSharp.Utils
{
    public class IpUtil
    {
        public static string GetLocalIp()
        {
            var hostName = Dns.GetHostName();
            var ipEntry = Dns.GetHostEntry(hostName);
            foreach (var ipAddress in ipEntry.AddressList)
            {
                if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ipAddress.ToString();
                }
            }

            return null;
        }
    }
}