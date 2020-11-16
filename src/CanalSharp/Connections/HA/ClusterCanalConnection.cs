using System;
using System.Text;
using System.Threading.Tasks;
using CanalSharp.Connections.Enums;
using CanalSharp.Protocol;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NZookeeper;
using NZookeeper.Enums;

namespace CanalSharp.Connections
{
    /// <summary>
    /// Support canal server cluster and client cluster.
    /// </summary>
    public class ClusterCanalConnection : ICanalConnection
    {
        private readonly ClusterCanalOptions _options;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<ClusterCanalConnection> _logger;
        // ReSharper disable once InconsistentNaming
        private readonly string ZK_RUNNING_NODE;

        // For reconnection
        private string _lastSubFilter;
        private bool _serverRunningNodeReCreated;
        private SimpleCanalConnection _currentConn;
        private ZkConnection _zk;

        public ClusterCanalConnection([NotNull] ClusterCanalOptions options, ILoggerFactory loggerFactory)
        {
            _options = options;
            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger<ClusterCanalConnection>();
            ZK_RUNNING_NODE = $"/otter/canal/destinations/{_options.Destination}/running";
        }


        public async Task ReConnectAsync()
        {
            await DisConnectAsync();

            await ConnectToZkAsync();

            var times = 0;
            while (!_serverRunningNodeReCreated && times < 120)
            {
                await Task.Delay(1000);
                times++;
                _logger.LogWarning($"Wait for the server cluster to recover. Waiting... {times}-120");
            }

            try
            {
                await ConnectAsync();
                _logger.LogInformation("Reconnect success.");

                if (_lastSubFilter != null)
                {
                    await SubscribeAsync(_lastSubFilter);
                    await RollbackAsync(0);
                    _logger.LogInformation($"Re subscription {_lastSubFilter} succeeded");
                }
            }
            catch (Exception e)
            {
                throw new CanalConnectionException("Reconnect failed.", e);

            }
        }

        public async Task ConnectAsync()
        {
            await ConnectToZkAsync();
            //get canal address from zk
            var nodeData = await _zk.GetDataAsync(ZK_RUNNING_NODE);
            var runningInfo = JsonConvert.DeserializeObject<CanalRunningInfo>(Encoding.UTF8.GetString(nodeData));
            _logger.LogInformation($"get canal address from zookeeper success: {runningInfo.Address}");

            //connect to canal
            _currentConn = new SimpleCanalConnection(CopyOptions(runningInfo), _loggerFactory.CreateLogger<SimpleCanalConnection>());
            await _currentConn.ConnectAsync();

            _serverRunningNodeReCreated = false;
        }

        private async Task ConnectToZkAsync()
        {
            if (_zk == null || !_zk.Connected)
            {
                if (_zk != null)
                {
                    await _zk.DisposeAsync();
                }

                _zk = new ZkConnection(
                    new ZkConnectionOptions()
                    { ConnectionString = _options.ZkAddress, SessionTimeout = _options.ZkSessionTimeout },
                    _loggerFactory.CreateLogger<ZkConnection>());
                _zk.OnWatch += Zk_OnWatch;
                await _zk.ConnectAsync();
            }

            var times = 0;
            while (times < 60)
            {
                if (await _zk.NodeExistsAsync(ZK_RUNNING_NODE))
                {
                    return;
                }

                await Task.Delay(1000);
                times++;

                _logger.LogWarning($"Can not find node {ZK_RUNNING_NODE} on Zookeeper. Retrying... {times}");
            }

            throw new CanalConnectionException($"Can not find node {ZK_RUNNING_NODE} on Zookeeper.");
        }

        private async Task Zk_OnWatch(ZkWatchEventArgs args)
        {
            if (args.Path == ZK_RUNNING_NODE && args.EventType == WatchEventType.NodeCreated)
            {
                _serverRunningNodeReCreated = true;
                _logger.LogInformation($"Zookeeper node {ZK_RUNNING_NODE} Created");
            }
            else if (args.Path == ZK_RUNNING_NODE && args.EventType == WatchEventType.NodeDeleted)
            {
                _logger.LogInformation($"Zookeeper node {ZK_RUNNING_NODE} Deleted");
            }

            await _zk.NodeExistsAsync(ZK_RUNNING_NODE);
        }

        public Task SubscribeAsync(string filter = ".*\\..*")
        {
            _lastSubFilter = filter;
            return _currentConn.SubscribeAsync(filter);
        }

        public Task UnSubscribeAsync(string filter = ".*\\..*")
        {
            return _currentConn.UnSubscribeAsync(filter);
        }

        public async Task DisConnectAsync()
        {
            if (_currentConn != null)
            {
                await _currentConn.DisposeAsync();
                _currentConn = null;
            }

            if (_zk != null)
            {
                await _zk.DisposeAsync();
                _zk = null;
            }

            _serverRunningNodeReCreated = false;
            _logger.LogInformation("Disconnect success.");
        }

        public async ValueTask DisposeAsync()
        {
            _lastSubFilter = null;
            await DisConnectAsync();

        }

        public Task AckAsync(long batchId)
        {
            return _currentConn.AckAsync(batchId);
        }

        public Task RollbackAsync(long batchId)
        {
            return _currentConn.RollbackAsync(batchId);
        }

        public Task<Message> GetAsync(int fetchSize)
        {
            return _currentConn.GetAsync(fetchSize);
        }

        public Task<Message> GetAsync(int fetchSize, long? timeout,
            FetchDataTimeoutUnitType timeOutUnit = FetchDataTimeoutUnitType.Millisecond)
        {
            return _currentConn.GetAsync(fetchSize, timeout);
        }

        public Task<Message> GetWithoutAckAsync(int fetchSize)
        {
            return _currentConn.GetWithoutAckAsync(fetchSize);
        }

        public Task<Message> GetWithoutAckAsync(int fetchSize, long? timeout,
            FetchDataTimeoutUnitType timeOutUnit = FetchDataTimeoutUnitType.Millisecond)
        {
            return _currentConn.GetWithoutAckAsync(fetchSize, timeout);
        }

        private SimpleCanalOptions CopyOptions(CanalRunningInfo runningInfo)
        {
            var tmpArr = runningInfo.Address.Split(":");
            var op = new SimpleCanalOptions(tmpArr[0], int.Parse(tmpArr[1]), _options.ClientId)
            {
                Destination = _options.Destination,
                IdleTimeout = _options.IdleTimeout,
                LazyParseEntry = _options.LazyParseEntry,
                Password = _options.Password,
                SoTimeout = _options.SoTimeout,
                UserName = _options.UserName
            };
            return op;
        }
    }
}