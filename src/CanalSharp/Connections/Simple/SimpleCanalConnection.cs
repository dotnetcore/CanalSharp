using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using BeetleX;
using BeetleX.Clients;
using CanalSharp.Connections.Enums;
using CanalSharp.Protocol;
using CanalSharp.Utils;
using Google.Protobuf;
using Microsoft.Extensions.Logging;

namespace CanalSharp.Connections
{
    /// <summary>
    /// Simple connection, suitable for business scenarios with small data traffic. High Availability is not supported.
    /// </summary>
    public class SimpleCanalConnection
    {
        private readonly SimpleCanalOptions _options;
        private readonly ILogger _logger;
        private TcpClient _client;

        public ConnectionState State { get; internal set; }

        public SimpleCanalConnection([NotNull] SimpleCanalOptions options, ILogger<SimpleCanalConnection> logger)
        {
            Check.NotNull(options, nameof(options));

            _options = options;
            _logger = logger;
            State = ConnectionState.Closed;
        }

        /// <summary>
        /// Connect to canal server.
        /// </summary>
        /// <returns></returns>
        public async Task ConnectAsync()
        {
            ValidateState(ConnectionState.Closed, nameof(ConnectAsync));

            _client = SocketFactory.CreateClient<TcpClient>(_options.Host, _options.Port);
            var p = await _client.ReadPacketAsync();
            // _client.Socket.ReceiveTimeout = _options.SoTimeout;
            //Handshake
            ValidatePackageType(p, PacketType.Handshake, 1);
            var handshake = Handshake.Parser.ParseFrom(p.Body);
            var seed = handshake.Seeds;

            var newPassword = _options.Password;
            if (_options.Password != null)
            {
                // encode password
                newPassword = SecurityUtil.ByteArrayToHexString(SecurityUtil.Scramble411(Encoding.UTF8.GetBytes(_options.Password), seed.ToByteArray()));
            }

            //Auth
            var data = new ClientAuth()
            {
                Username = _options.UserName,
                Password = ByteString.CopyFromUtf8(newPassword ?? ""),
                NetReadTimeout = _options.IdleTimeout,
                NetWriteTimeout = _options.IdleTimeout
            };

            var packet = new Packet()
            {
                Type = PacketType.Clientauthentication,
                Body = data.ToByteString()
            }.ToByteArray();

            //Send auth data
            await _client.WritePacketAsync(packet);
            p = await _client.ReadPacketAsync();

            //Validate auth ack packet
            p.ValidateAck();

            //Set state 
            SetConnectionState(ConnectionState.Connected);

            _logger.LogInformation($"Connect to canal server [{_options.Host}:{_options.Port}] success.");

        }

        /// <summary>
        /// Subscribe to the data that needs to be received. Support repeat subscription to refresh filter setting.
        /// </summary>
        /// <param name="filter">Subscribe filter(Optional, default value is '.*\\..*')</param>
        /// <returns></returns>
        public async Task SubscribeAsync(string filter = ".*\\..*")
        {
            ValidateState(ConnectionState.Connected | ConnectionState.Subscribed | ConnectionState.Unsubscribed, nameof(SubscribeAsync));

            //Sub data
            var data = new Sub()
            {
                Destination = _options.Destination,
                ClientId = _options.ClientId,
                Filter = filter
            };

            //Assemble data package
            var pack = new Packet()
            {
                Type = PacketType.Subscription,
                Body = data.ToByteString()
            }.ToByteArray();

            //Send packet
            await _client.WritePacketAsync(pack);
            //Receive response
            var p = await _client.ReadPacketAsync();
            //Validate ack packet
            p.ValidateAck();
            //Set state
            SetConnectionState(ConnectionState.Subscribed);

            _logger.LogInformation($"Subscribe {filter} success.");
        }

        /// <summary>
        /// UnSubscribe. If you need to unsubscribe, you should stop receiving data first.
        /// </summary>
        /// <param name="filter">UnSubscribe filter(Optional, default value is '.*\\..*')</param>
        /// <returns></returns>
        public async Task UnSubscribeAsync(string filter = ".*\\..*")
        {
            ValidateState(ConnectionState.Subscribed, nameof(UnSubscribeAsync));

            //Unsub data
            var data = new Unsub()
            {
                Destination = _options.Destination,
                ClientId = _options.ClientId,
                Filter = filter
            };

            var pack = new Packet()
            {
                Type = PacketType.Subscription,
                Body = data.ToByteString()
            }.ToByteArray();

            await _client.WritePacketAsync(pack);
            var p = await _client.ReadPacketAsync();
            p.ValidateAck();

            SetConnectionState(ConnectionState.Unsubscribed);

            _logger.LogInformation($"UnSubscribe {filter} success.");
        }

        /// <summary>
        /// Fetch data from Canal Server, have auto Ack and no timeout
        /// </summary>
        /// <param name="fetchSize">Fetch data Size. If you set the value to null or less than or equal to 0, the value will be reset to 1000. </param>
        /// <returns></returns>
        public Task<Message> GetAsync(int fetchSize)
        {
            return GetAsync(fetchSize, null);
        }

        /// <summary>
        /// Fetch data from Canal Server, have auto Ack
        /// </summary>
        /// <param name="fetchSize">Fetch data Size. If you set the value to null or less than or equal to 0, the value will be reset to 1000. </param>
        /// <param name="timeout">Fetch data timeout. If you set the value to null or less than or equal to 0, the value will be reset to -1. -1 means no timeout.</param>
        /// <param name="timeOutUnit">Timeout unit. Default value is <see cref="FetchDataTimeoutUnitType.Millisecond"/>.</param>
        /// <returns></returns>
        public async Task<Message> GetAsync(int fetchSize, long? timeout,
            FetchDataTimeoutUnitType timeOutUnit = FetchDataTimeoutUnitType.Millisecond)
        {
            var message = await GetWithoutAckAsync(fetchSize, timeout, timeOutUnit);
            await AckAsync(message.Id);
            return message;
        }


        /// <summary>
        /// Fetch data from Canal Server, but have no auto Ack and no timeout
        /// </summary>
        /// <param name="fetchSize">Fetch data Size. If you set the value to null or less than or equal to 0, the value will be reset to 1000. </param>
        /// <returns></returns>
        public async Task<Message> GetWithoutAckAsync(int fetchSize)
        {
            return await GetWithoutAckAsync(fetchSize, null);
        }

        /// <summary>
        /// Fetch data from Canal Server, but have no auto Ack
        /// </summary>
        /// <param name="fetchSize">Fetch data Size. If you set the value to null or less than or equal to 0, the value will be reset to 1000. </param>
        /// <param name="timeout">Fetch data timeout. If you set the value to null or less than or equal to 0, the value will be reset to -1. -1 means no timeout.</param>
        /// <param name="timeOutUnit">Timeout unit. Default value is <see cref="FetchDataTimeoutUnitType.Millisecond"/>.</param>
        /// <returns></returns>
        public async Task<Message> GetWithoutAckAsync(int fetchSize, long? timeout, FetchDataTimeoutUnitType timeOutUnit = FetchDataTimeoutUnitType.Millisecond)
        {
            ValidateState(ConnectionState.Subscribed, nameof(GetWithoutAckAsync));

            var size = fetchSize <= 0 ? 1000 : fetchSize;
            var time = (timeout == null || timeout < 0) ? -1 : timeout;

            var get = new Get()
            {
                AutoAck = false,
                Destination = _options.Destination,
                ClientId = _options.ClientId,
                FetchSize = size,
                Timeout = (long)time,
                Unit = (int)timeOutUnit
            };

            var pack = new Packet()
            {
                Type = PacketType.Get,
                Body = get.ToByteString()
            }.ToByteArray();

            await _client.WritePacketAsync(pack);
            var p = await _client.ReadPacketAsync();

            return DeserializeMessage(p);
        }

        /// <summary>
        /// Ack has received data.
        /// </summary>
        /// <param name="batchId"></param>
        /// <returns></returns>
        public async Task AckAsync(long batchId)
        {
            ValidateState(ConnectionState.Subscribed, nameof(AckAsync));

            var ca = new ClientAck()
            {
                Destination = _options.Destination,
                ClientId = _options.ClientId,
                BatchId = batchId
            };

            var pack = new Packet()
            {
                Type = PacketType.Clientack,
                Body = ca.ToByteString()
            }.ToByteArray();

            await _client.WritePacketAsync(pack);

            _logger.LogDebug($"Ack {batchId} success.");
        }

        /// <summary>
        /// Roll back consumption progress
        /// </summary>
        /// <param name="batchId"></param>
        /// <returns></returns>
        public async Task RollbackAsync(long batchId)
        {
            ValidateState(ConnectionState.Subscribed, nameof(RollbackAsync));

            var ca = new ClientRollback()
            {
                Destination = _options.Destination,
                ClientId = _options.ClientId,
                BatchId = batchId
            };

            var pack = new Packet()
            {
                Type = PacketType.Clientrollback,
                Body = ca.ToByteString()
            }.ToByteArray();

            await _client.WritePacketAsync(pack);

            _logger.LogDebug($"Rollback {batchId} success.");
        }

        /// <summary>
        /// Close connection
        /// </summary>
        public Task DisConnectAsync()
        {
            _client?.DisConnect();
            SetConnectionState(ConnectionState.Closed);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Disconnect TCP connection and dispose 
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            await DisConnectAsync();
            _client?.Dispose();
        }

        private Message DeserializeMessage(Packet p)
        {
            // Check packet type

            if (p.Type == PacketType.Ack)
            {
                var ackBody = Ack.Parser.ParseFrom(p.Body);
                throw new CanalConnectionException("Received an error returned by the server: " +
                                                   ackBody.ErrorMessage);
            }

            if (p.Type != PacketType.Messages)
            {
                throw new CanalConnectionException($"Unexpected packet type received during fetch data: {p.Type}");
            }

            if (p.Compression != Compression.None && p.Compression != Compression.Compatibleproto2)
            {
                throw new CanalConnectionException($"Compression is not supported in this connection: {p.Compression}");
            }

            var messages = Messages.Parser.ParseFrom(p.Body);
            Message result;
            if (_options.LazyParseEntry)
            {
                result=new Message(messages.BatchId,true, messages.Messages_.ToList());
            }
            else
            {
                result=new Message(messages.BatchId);
                foreach (var byteString in messages.Messages_)
                {
                    result.AddEntry(Entry.Parser.ParseFrom(byteString));
                }
            }
            return result;
        }

        internal EndPoint GetLocalEndPoint()
        {
            return _client.Socket.LocalEndPoint;
        }

        private void ValidatePackageType(Packet p, PacketType expectPacketType, int expectPacketVersion)
        {
            if (p.Type != expectPacketType)
            {
                throw new CanalConnectionException($"Unexpected packet type received, expect: {expectPacketType}, actually: {p.Type}");
            }

            if (p.Version != 1)
            {
                throw new CanalConnectionException($"Unexpected packet version received, expect: {expectPacketVersion}, actually: {p.Version}");
            }
        }

        private void ValidateState(ConnectionState expectState, string methodName)
        {
            if ((State & expectState) != State)
            {
                throw new CanalConnectionException($"Connection State is {State}, expect {expectState}, can not execute {methodName}.");
            }
        }

        private void SetConnectionState(ConnectionState state)
        {
            State = state;
        }

    }
}