
using System;
using CanalSharp.Protocol;
using Microsoft.Extensions.Logging;
namespace CanalSharp.Client.Impl
{
    public class SimpleCanalConnector: ICanalConnector
    {
        private  ILogger _logger;

        public SimpleCanalConnector(ILogger<SimpleCanalConnector> logger)
        {
            _logger = logger;
        }

        public void Connect()
        {

            throw new NotImplementedException();
        }

        public void Disconnect()
        {
            throw new NotImplementedException();
        }

        public bool CheckValid()
        {
            throw new NotImplementedException();
        }

        public void Subscribe(string filter)
        {
            throw new NotImplementedException();
        }

        public void Subscribe()
        {
            throw new NotImplementedException();
        }

        public void UnSubscribe()
        {
            throw new NotImplementedException();
        }

        public Message Get(int batchSize)
        {
            throw new NotImplementedException();
        }

        public Message Get(int batchSize, long timeout, TimeSpan unit)
        {
            throw new NotImplementedException();
        }

        public Message GetWithoutAck(int batchSize)
        {
            throw new NotImplementedException();
        }

        public Message GetWithoutAck(int batchSize, long timeout, TimeSpan unit)
        {
            throw new NotImplementedException();
        }

        public void Ack(long batchId)
        {
            throw new NotImplementedException();
        }

        public void Rollback(long batchId)
        {
            throw new NotImplementedException();
        }

        public void Rollback()
        {
            throw new NotImplementedException();
        }

        public void StopRunning()
        {
            throw new NotImplementedException();
        }
    }
}