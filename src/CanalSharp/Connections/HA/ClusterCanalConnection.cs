using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CanalSharp.Connections.Enums;
using CanalSharp.Protocol;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NZookeeper;
using NZookeeper.ACL;
using NZookeeper.Enums;
using NZookeeper.Node;
using org.apache.zookeeper;

namespace CanalSharp.Connections
{
    /// <summary>
    /// Support canal server cluster and client cluster.
    /// </summary>
    public class ClusterCanalConnection
    {
        private readonly ClusterCanalOptions _options;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<ClusterCanalConnection> _logger;
        private readonly TaskCompletionSource<int> _completionSource;
        // ReSharper disable once InconsistentNaming
        private readonly string ZK_SERVER_RUNNING_NODE;
        // ReSharper disable once InconsistentNaming
        private readonly string ZK_CLIENT_RUNNING_NODE;

        // For reconnect
        private string _lastSubFilter;
        private bool _serverRunningNodeReCreated;
        private SimpleCanalConnection _currentConn;
        private ZkConnection _zk;
        private CanalClientRunningInfo _clientRunningInfo;

        public ClusterCanalConnection([NotNull] ClusterCanalOptions options, ILoggerFactory loggerFactory)
        {
            _options = options;
            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger<ClusterCanalConnection>();
            _completionSource = new TaskCompletionSource<int>();
            ZK_SERVER_RUNNING_NODE = $"/otter/canal/destinations/{_options.Destination}/running";
            ZK_CLIENT_RUNNING_NODE = $"/otter/canal/destinations/{_options.Destination}/{_options.ClientId}/running";
        }

        public ConnectionState State => GetState();

        private ConnectionState GetState()
        {
            return _currentConn?.State ?? ConnectionState.Closed;
        }
        /// <summary>
        /// Reconnect to canal server.
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Connect to canal server.
        /// </summary>
        /// <returns></returns>
        public async Task ConnectAsync()
        {
            //get canal address from zk
            await ConnectToZkAsync();
            var nodeData = await _zk.GetDataAsync(ZK_SERVER_RUNNING_NODE);
            var runningInfo = JsonConvert.DeserializeObject<CanalServerRunningInfo>(Encoding.UTF8.GetString(nodeData));
            _logger.LogInformation($"get canal address from zookeeper success: {runningInfo.Address}");

            //connect to canal
            _currentConn = new SimpleCanalConnection(CopyOptions(runningInfo), _loggerFactory.CreateLogger<SimpleCanalConnection>());
            await _currentConn.ConnectAsync();
            _serverRunningNodeReCreated = false;

            var localIp = _currentConn.GetLocalEndPoint().ToString();
            _clientRunningInfo = new CanalClientRunningInfo()
            { Active = true, Address = localIp, ClientId = _options.ClientId };
            _ = GetZkLockAsync(_clientRunningInfo);
            await _completionSource.Task;

            _logger.LogInformation("Ready to use!");
        }

        private async Task GetZkLockAsync(CanalClientRunningInfo runningInfo, bool waiting = false)
        {
            var times = 0;
            while (waiting && times < 60)
            {
                await Task.Delay(1000);
                times++;
                _logger.LogWarning($"Waiting for get lock {times}-60...");
            }

            if (await _zk.NodeExistsAsync(ZK_CLIENT_RUNNING_NODE))
            {
                _logger.LogInformation($"Node {ZK_CLIENT_RUNNING_NODE} exits, get Zookeeper lock failed. Other instances are running.");
                _logger.LogWarning("Waiting...");
            }
            else
            {
                try
                {
                    var clientNodeData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(runningInfo));

                    var parentNode = ZK_CLIENT_RUNNING_NODE.Replace("/running", "");
                    if (!await _zk.NodeExistsAsync(parentNode))
                        await _zk.CreateNodeAsync(parentNode, null,
                        new List<Acl>() { new Acl(AclPerm.All, AclScheme.World, AclId.World()) }, NodeType.Persistent);

                    await _zk.CreateNodeAsync(ZK_CLIENT_RUNNING_NODE, clientNodeData,
                        new List<Acl>() { new Acl(AclPerm.All, AclScheme.World, AclId.World()) }, NodeType.Ephemeral);
                    await _zk.CreateNodeAsync(parentNode + "/" + runningInfo.Address, null,
                        new List<Acl>() { new Acl(AclPerm.All, AclScheme.World, AclId.World()) }, NodeType.Ephemeral);
                    await _zk.NodeExistsAsync(ZK_CLIENT_RUNNING_NODE);
                    _completionSource.SetResult(0);
                    _logger.LogInformation("Get zookeeper lock success.");
                }
                catch (KeeperException.NodeExistsException e)
                {
                    if (e.getPath() != ZK_CLIENT_RUNNING_NODE)
                    {
                        _logger.LogError(e, "Error during get lock.");
                        Environment.Exit(-1);
                    }

                    _logger.LogInformation(
                        $"Node {ZK_CLIENT_RUNNING_NODE} exits, get Zookeeper lock failed. Other instances are running.");
                    _logger.LogWarning("Waiting...");
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Exception in lock");
                }
            }

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
                if (await _zk.NodeExistsAsync(ZK_SERVER_RUNNING_NODE))
                {
                    return;
                }

                await Task.Delay(1000);
                times++;

                _logger.LogWarning($"Can not find node {ZK_SERVER_RUNNING_NODE} on Zookeeper. Retrying... {times}");
            }

            throw new CanalConnectionException($"Can not find node {ZK_SERVER_RUNNING_NODE} on Zookeeper.");
        }

        private async Task Zk_OnWatch(ZkWatchEventArgs args)
        {
            if (args.Path == ZK_SERVER_RUNNING_NODE && args.EventType == WatchEventType.NodeCreated)
            {
                _serverRunningNodeReCreated = true;
                _logger.LogInformation($"Server node {ZK_SERVER_RUNNING_NODE} Created");
            }

            if (args.Path == ZK_SERVER_RUNNING_NODE && args.EventType == WatchEventType.NodeDeleted)
            {
                _logger.LogInformation($"Server node {ZK_SERVER_RUNNING_NODE} Deleted");
            }

            if (args.Path == ZK_CLIENT_RUNNING_NODE && args.EventType == WatchEventType.NodeDeleted)
            {
                _logger.LogInformation($"Client node {ZK_SERVER_RUNNING_NODE} Deleted");
                _ = GetZkLockAsync(_clientRunningInfo, true);
            }

            if (_zk.Connected)
            {
                await _zk.NodeExistsAsync(ZK_SERVER_RUNNING_NODE);
                await _zk.NodeExistsAsync(ZK_CLIENT_RUNNING_NODE);
            }
        }

        /// <summary>
        /// Subscribe to the data that needs to be received. Support repeat subscription to refresh filter setting.
        /// </summary>
        /// <param name="filter">Subscribe filter(Optional, default value is '.*\\..*')</param>
        /// <returns></returns>
        public Task SubscribeAsync(string filter = ".*\\..*")
        {
            _lastSubFilter = filter;
            return _currentConn.SubscribeAsync(filter);
        }

        /// <summary>
        /// UnSubscribe. If you need to unsubscribe, you should stop receiving data first.
        /// </summary>
        /// <param name="filter">UnSubscribe filter(Optional, default value is '.*\\..*')</param>
        /// <returns></returns>
        public Task UnSubscribeAsync(string filter = ".*\\..*")
        {
            return _currentConn.UnSubscribeAsync(filter);
        }

        /// <summary>
        /// Close connection
        /// </summary>  
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

        /// <summary>
        /// Disconnect TCP connection and dispose 
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            _lastSubFilter = null;
            await DisConnectAsync();

        }

        /// <summary>
        /// Ack has received data.
        /// </summary>
        /// <param name="batchId"></param>
        /// <returns></returns>
        public Task AckAsync(long batchId)
        {
            return _currentConn.AckAsync(batchId);
        }

        /// <summary>
        /// Roll back consumption progress
        /// </summary>
        /// <param name="batchId"></param>
        /// <returns></returns>
        public Task RollbackAsync(long batchId)
        {
            return _currentConn.RollbackAsync(batchId);
        }

        /// <summary>
        /// Fetch data from Canal Server, have auto Ack and no timeout
        /// </summary>
        /// <param name="fetchSize">Fetch data Size. If you set the value to null or less than or equal to 0, the value will be reset to 1000. </param>
        /// <returns></returns>
        public Task<Message> GetAsync(int fetchSize)
        {
            return _currentConn.GetAsync(fetchSize);
        }

        /// <summary>
        /// Fetch data from Canal Server, have auto Ack
        /// </summary>
        /// <param name="fetchSize">Fetch data Size. If you set the value to null or less than or equal to 0, the value will be reset to 1000. </param>
        /// <param name="timeout">Fetch data timeout. If you set the value to null or less than or equal to 0, the value will be reset to -1. -1 means no timeout.</param>
        /// <param name="timeOutUnit">Timeout unit. Default value is <see cref="FetchDataTimeoutUnitType.Millisecond"/>.</param>
        /// <returns></returns>
        public Task<Message> GetAsync(int fetchSize, long? timeout,
            FetchDataTimeoutUnitType timeOutUnit = FetchDataTimeoutUnitType.Millisecond)
        {
            return _currentConn.GetAsync(fetchSize, timeout);
        }

        /// <summary>
        /// Fetch data from Canal Server, but have no auto Ack and no timeout
        /// </summary>
        /// <param name="fetchSize">Fetch data Size. If you set the value to null or less than or equal to 0, the value will be reset to 1000. </param>
        /// <returns></returns>
        public Task<Message> GetWithoutAckAsync(int fetchSize)
        {
            return _currentConn.GetWithoutAckAsync(fetchSize);
        }

        /// <summary>
        /// Fetch data from Canal Server, but have no auto Ack
        /// </summary>
        /// <param name="fetchSize">Fetch data Size. If you set the value to null or less than or equal to 0, the value will be reset to 1000. </param>
        /// <param name="timeout">Fetch data timeout. If you set the value to null or less than or equal to 0, the value will be reset to -1. -1 means no timeout.</param>
        /// <param name="timeOutUnit">Timeout unit. Default value is <see cref="FetchDataTimeoutUnitType.Millisecond"/>.</param>
        /// <returns></returns>
        public Task<Message> GetWithoutAckAsync(int fetchSize, long? timeout,
            FetchDataTimeoutUnitType timeOutUnit = FetchDataTimeoutUnitType.Millisecond)
        {
            return _currentConn.GetWithoutAckAsync(fetchSize, timeout);
        }

        private SimpleCanalOptions CopyOptions(CanalServerRunningInfo runningInfo)
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