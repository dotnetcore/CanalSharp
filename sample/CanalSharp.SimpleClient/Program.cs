using System;
using CanalSharp.Client;
using CanalSharp.Client.Impl;

namespace CanalSharp.SimpleClient
{
    class Program
    {
        static void Main(string[] args)
        {
            string destination = "example";
            ICanalConnector connector = CanalConnectors.NewSingleConnector("127.0.0.1", 11111, destination, "", "");
            connector.Connect();
            Console.Read();
        }
    }
}
