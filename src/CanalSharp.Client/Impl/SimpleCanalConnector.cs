using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using CanalSharp.Common.Logging;
using CanalSharp.Protocol;
using CanalSharp.Protocol.Exception;
using Com.Alibaba.Otter.Canal.Protocol;
using DotNetty.Buffers;
using DotNetty.Common.Utilities;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Google.Protobuf;

namespace CanalSharp.Client.Impl
{
    public class SimpleCanalConnector : ChannelHandlerAdapter, ICanalConnector
    {
        private readonly ILogger _logger = LogManager.GetLogger(typeof(SimpleCanalConnector));
        public string Address { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        /// <summary>
        ///  // milliseconds
        /// </summary>
        public int SoTimeout { get; set; } = 60000;

        /// <summary>
        /// client和server之间的空闲链接超时的时间,默认为1小时
        /// </summary>
        public int IdleTimeout { get; set; } = 60 * 60 * 1000;

        private ClientIdentity _clientIdentity;
        /// <summary>
        ///  // 代表connected是否已正常执行，因为有HA，不代表在工作中
        /// </summary>
        private volatile bool _connected = false;

        //private ClientRunningMonitor runningMonitor;

        private volatile bool _running = false;
        /// <summary>
        /// 记录上一次的filter提交值,便于自动重试时提交
        /// </summary>
        private string _filter;

        private ISocketChannel _channel;

        private Task<IChannel> readableChannel;

        private IChannel writableChannel;

        private List<Compression> supportedCompressions = new List<Compression>();

        private static readonly object _writeDataLock = new object();
        private static readonly object _readDataLock = new object();



        private IByteBuffer readHeader = Unpooled.Buffer(1024);
        private IByteBuffer writeHeader = Unpooled.Buffer(1024);

        private IChannel _clientChannel;
        private IChannel _testChannel;

        // 是否在connect链接成功后，自动执行rollback操作
        private bool _rollbackOnConnect = true;

        private Message _message;
        // 是否自动化解析Entry对象,如果考虑最大化性能可以延后解析
        private bool _lazyParseEntry = false;



        public SimpleCanalConnector(string address, int port, string username, string password, string destination) : this(address, port, username, password, destination, 60000, 60 * 60 * 1000)
        {

        }

        public SimpleCanalConnector(string address, int port, string username, string password, string destination,
            int soTimeout) : this(address, port, username, password, destination, soTimeout, 60 * 60 * 1000)
        {

        }

        public SimpleCanalConnector(string address, int port, string username, string password, string destination,
            int soTimeout, int idleTimeout)
        {
            Address = address;
            Port = port;
            Username = username;
            Password = password;
            SoTimeout = soTimeout;
            IdleTimeout = idleTimeout;
            _clientIdentity = new ClientIdentity(destination, (short)1001);
        }

        public  void Connect()
        {
            if (_connected)
            {
                return;
            }

            //WaitClientRunning();
            if (_running)
            {
                return;
            }

             DoConnect().Wait();
            if (_filter != null)
            { // 如果存在条件，说明是自动切换，基于上一次的条件订阅一次
                Subscribe(_filter);
            }
            //if (_rollbackOnConnect)
            //{
            //    Rollback();
            //}
            _connected = true;
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
            //waitClientRunning();
            if (!_running)
            {
                return;
            }

            try
            {
                var pack = new Packet()
                {
                    Type = PacketType.Subscription,
                    Body = new Sub()
                    {
                        Destination = _clientIdentity.Destination,
                        ClientId = _clientIdentity.ClientId.ToString(),
                        Filter = _filter != null ? _filter : ""
                    }.ToByteString()
                };
                WriteWithHeader(_channel, pack.ToByteArray());

                //Packet p = Packet.parseFrom(readNextPacket());
                //Ack ack = Ack.parseFrom(p.getBody());
                //if (ack.getErrorCode() > 0)
                //{
                //    throw new CanalClientException("failed to subscribe with reason: " + ack.getErrorMessage());
                //}

                _clientIdentity.Filter = filter;
            }
            catch (IOException e)
            {
                throw new CanalClientException(e.Message);
            }

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
            return GetWithoutAck(batchSize, null, null);
        }

        public Message Get(int batchSize, long timeout, TimeSpan unit)
        {
            throw new NotImplementedException();
        }

        public Message GetWithoutAck(int batchSize)
        {
           return GetWithoutAck(batchSize, null, null);
        }

        public  Message GetWithoutAck(int batchSize, long? timeout, int? unit)
        {
            //waitClientRunning();
            //if (!_running)
            //{
            //    return null;
            //}

            try
            {
                lock (this)
                {
                    var size = (batchSize <= 0) ? 1000 : batchSize;
                // -1代表不做timeout控制
                var time = (timeout == null || timeout < 0) ? -1 : timeout;
                if (unit == null)
                {
                    unit = 1;
                }
                var get = new Get()
                {
                    AutoAck = false,
                    Destination = _clientIdentity.Destination,
                    ClientId = _clientIdentity.ClientId.ToString(),
                    FetchSize = size,
                    Timeout = (long)time,
                    Unit = (int)unit
                };
                var packet = new Packet()
                {
                    Type = PacketType.Get,
                    Body = get.ToByteString()
                }.ToByteArray();

                WriteWithHeaderA(_clientChannel, packet);
                Monitor.Wait(this);
                }



                return _message;


            }
            catch (IOException e)
            {
                throw e;
            }
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
        private async Task DoConnect()
        {
            var group = new MultithreadEventLoopGroup();
            var bootstrap = new Bootstrap();
            bootstrap
                .Group(group)
                .Channel<TcpSocketChannel>()
                .Option(ChannelOption.SoTimeout, SoTimeout)
                .Option(ChannelOption.TcpNodelay, true)
                .Option(ChannelOption.SoKeepalive, true)
                .Option(ChannelOption.Allocator, UnpooledByteBufferAllocator.Default)
                .Handler(new ActionChannelInitializer<ISocketChannel>(channel =>
                {
                    var pipeline = channel.Pipeline;
                    pipeline.AddLast(nameof(SimpleCanalConnector), this);
                }));
            _clientChannel =await bootstrap.ConnectAsync(new IPEndPoint(IPAddress.Parse(Address), Port)).ConfigureAwait(false);
       }

        private void WaitClientRunning()
        {
            _running = true;
        }


        private void WriteWithHeader(IChannel channel, byte[] body)
        {
            lock (_writeDataLock)
            {
                writeHeader.Clear();
                writeHeader.WriteInt(body.Length);
                channel.WriteAsync(writeHeader);
                channel.WriteAndFlushAsync(Unpooled.WrappedBuffer(body)).Wait();

            }
        }

        private void WriteWithHeaderA(IChannel channel, byte[] body)
        {
            lock (_writeDataLock)
            {
                readHeader.Clear();
                readHeader.WriteInt(body.Length);
                channel.WriteAsync(readHeader);
                channel.WriteAndFlushAsync(Unpooled.WrappedBuffer(body)).Wait();

            }
        }

        private void WriteWithHeader(byte[] body)
        {
            WriteWithHeader(writableChannel, body);
        }


        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            var byteBuffer = (IByteBuffer)message;
            if (byteBuffer != null)
            {
                var result = byteBuffer.Array.Slice(4, 2);
                var p = Packet.Parser.ParseFrom(result);
                switch (p.Type)
                {
                    case PacketType.Handshake when p.Version != 1:
                        throw new CanalClientException("unsupported version at this client.");
                    case PacketType.Handshake when p.Type != PacketType.Handshake:
                        throw new CanalClientException("expect handshake but found other type.");
                    case PacketType.Handshake:
                        var handshake = Handshake.Parser.ParseFrom(p.Body);
                        supportedCompressions.Add(handshake.SupportedCompressions);

                        var ca = new ClientAuth()
                        {
                            Username = Username != null ? Username : "",
                            Password = ByteString.CopyFromUtf8(Password != null ? Password : ""),
                            NetReadTimeout = IdleTimeout,
                            NetWriteTimeout = IdleTimeout
                        };

                        WriteWithHeader(_clientChannel, new Packet()
                        {
                            Type = PacketType.Clientauthentication,
                            Body = ca.ToByteString()
                        }.ToByteArray());
                        break;
                    case PacketType.Ack when p.Type != PacketType.Ack:
                        throw new CanalClientException("unexpected packet type when ack is expected");
                    case PacketType.Ack:
                        var ackBody = Com.Alibaba.Otter.Canal.Protocol.Ack.Parser.ParseFrom(p.Body);
                        if (ackBody.ErrorCode > 0)
                        {
                            throw new CanalClientException($"something goes wrong when doing authentication:{ackBody.ErrorMessage} ");
                        }

                       new Thread(() => { GetWithoutAck(100, null, null); }).Start();
                        break;
                    case PacketType.Messages when !p.Compression.Equals(Compression.None):
                        throw new CanalClientException("compression is not supported in this connector");
                    //return msg;
                    case PacketType.Messages:
                        var messages = Messages.Parser.ParseFrom(p.Body);
                        var msg = new Message(messages.BatchId);
                        if (_lazyParseEntry)
                        {
                            // byteString
                            msg.RawEntries = messages.Messages_.ToList();
                        }
                        else
                        {
                            foreach (ByteString byteString in messages.Messages_)
                            {
                                msg.Entries.Add(Entry.Parser.ParseFrom(byteString));
                            }
                        }

                       
                        lock (this)
                        {
                            _message = msg;
                            Monitor.Pulse(this);
                        }
                        
                        break;
                }


                _connected = true;
            }

        }

        public override void ChannelReadComplete(IChannelHandlerContext context)
        {

        }
    }





}
