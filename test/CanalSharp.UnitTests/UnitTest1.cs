using System;
using System.Threading;
using CanalSharp.Client;
using CanalSharp.Client.Impl;
using Xunit;

namespace CanalSharp.UnitTests
{
    public class UnitTest1
    {
        private string aa;
        private static readonly object _lock = new object();
        [Fact]
        public void Test1()
        {
            string destination = "example";
            ICanalConnector connector = CanalConnectors.NewSingleConnector("127.0.0.1", 11111, destination, "", "");
            connector.Connect();
            Console.Read();

            Mutex a=new Mutex(false);
            //lock (_lock)
            //{
            //    new Thread(Test).Start();
            //    Monitor.Wait(_lock);
            //    Console.WriteLine(aa);
            //    Console.Read();
            //}

        }


        public void Test()
        {
            lock (_lock)
            {
                Thread.Sleep(500);
                aa = "aaaaa";
                Monitor.Pulse(_lock);
            }
            
        }
    }
}
