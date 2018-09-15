using System.Net;
using Canal.Csharp.Core;

namespace CanalSharp.Client.Impl
{
    public class CanalConnectors
    {
        /// <summary>
        /// 创建单链接的客户端链接
        /// </summary>
        /// <param name="address"></param>
        /// <param name="destination"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static ICanalConnector NewSingleConnector(string address, int port, string destination, string username,
            string password)
        {
            var canalConnector = new SimpleCanalConnector(address, port, username, password, destination)
            {
                SoTimeout = 60 * 1000,
                IdleTimeout = 60 * 60 * 1000
            };
            return canalConnector;
        }
    }
}
