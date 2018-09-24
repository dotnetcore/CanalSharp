using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using CanalSharp.Common.Logging;

namespace CanalSharp.Common.Utils
{

    public class AddressUtils
    {
        private static readonly ILogger _logger = LogManager.GetLogger(typeof(AddressUtils));
        private static  string  LOCALHOST_IP = "127.0.0.1";
        private static string EMPTY_IP     = "0.0.0.0";
        private static readonly Regex IP_PATTERN   = new Regex("[0-9]{1,3}(\\.[0-9]{1,3}){3,}");


        public static bool IsAvailablePort(int port)
        {
            TcpListener ss = null;
            try
            {
                ss = new TcpListener(port);
                ss.Start();
                return true;
            }
            catch (IOException e)
            {
                return false;
            }
            finally
            {
                if (ss != null)
                {
                    try
                    {
                        ss.Stop();
                    }
                    catch (IOException e)
                    {
                    }
                }
            }
        }

        private static bool IsValidHostAddress(IPAddress address)
        {
            if (address == null || IPAddress.IsLoopback(address)) return false;
            var name = Dns.GetHostEntry(address).HostName;
            return (name != null && !EMPTY_IP.Equals(name) && !LOCALHOST_IP.Equals(name) && IP_PATTERN.IsMatch(name));
        }


        public static string GetHostIp()
        {
            var address = GetHostAddress();
            return address == null ? null : Dns.GetHostEntry(address).HostName;
        }

        public static string GetHostName()
        {
            //IPHostEntry hostEntry;

            //hostEntry = Dns.GetHostEntry(host);

            ////you might get more than one ip for a hostname since 
            ////DNS supports more than one record

            //if (hostEntry.AddressList.Length > 0)
            //{
            //    var ip = hostEntry.AddressList[0];
            //    Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
            //    s.Connect(ip, 80);
            //}
            var address = GetHostAddress();
            return address == null ? null : Dns.GetHostEntry(address).HostName;
        }


        public static IPAddress GetHostAddress()
        {
            IPAddress localAddress = null;
            try
            {
                localAddress = Dns.GetHostEntry(Dns.GetHostName())
                    .AddressList.First(
                        f => f.AddressFamily == AddressFamily.InterNetwork);
                if (IsValidHostAddress(localAddress))
                {
                    return localAddress;
                }
            }
            catch (System.Exception e)
            {
                _logger.Warning($"Failed to retriving local host ip address, try scan network card ip address. cause: {e.Message}");
            }
            try
            {
                var interfaces = NetworkInterface.GetAllNetworkInterfaces();
                if (interfaces != null)
                {
                    foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
                    {
                        if (ni.NetworkInterfaceType != NetworkInterfaceType.Wireless80211 &&
                            ni.NetworkInterfaceType != NetworkInterfaceType.Ethernet) continue;
                        Console.WriteLine(ni.Name);
                        foreach (var ip in ni.GetIPProperties().UnicastAddresses)
                        {
                            if (ip.Address.AddressFamily != AddressFamily.InterNetwork) continue;
                            var address = ip.Address;
                            if (IsValidHostAddress(address))
                            {
                                return address;
                            }
                        }
                    }
                    
                }
            }
            catch (System.Exception e)
            {
                _logger.Warning($"Failed to retriving network card ip address. cause:{e.Message}");
            }
            _logger.Warning("Could not get local host ip address, will use 127.0.0.1 instead.");
            return localAddress;
        }

    }
}
