using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using CanalSharp.Common.Logging;
using CanalSharp.Protocol;
using CanalSharp.Protocol.Exception;
using Com.Alibaba.Otter.Canal.Protocol;
using DotNetty.Buffers;
using DotNetty.Codecs;
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



        private IByteBuffer readHeader = Unpooled.Buffer(4);
        private IByteBuffer writeHeader = Unpooled.Buffer(4);

        private IChannel _clientChannel;




        public SimpleCanalConnector(string address, int port, string username, string password, string destination) : this(address, port, username, password, destination, 60000, 60 * 60 * 1000)
        {

        }

        public SimpleCanalConnector(string address,int port, string username, string password, string destination,
            int soTimeout) : this(address, port, username, password, destination, soTimeout, 60 * 60 * 1000)
        {

        }

        public SimpleCanalConnector(string address,int port, string username, string password, string destination,
            int soTimeout, int idleTimeout)
        {
            Address = address;
            Port = port;
            Username = username;
            Password = password;
            SoTimeout = soTimeout;
            IdleTimeout = idleTimeout;
            _clientIdentity = new ClientIdentity(destination, (short)1001);
            Environment.SetEnvironmentVariable("io.netty.allocator.maxOrder", "4");
        }

        public async void Connect()
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

            await DoConnect();
            //if (_filter != null)
            //{ // 如果存在条件，说明是自动切换，基于上一次的条件订阅一次
            //    subscribe(filter);
            //}
            //if (rollbackOnConnect)
            //{
            //    rollback();
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
        private async Task DoConnect()
        {
            var group = new MultithreadEventLoopGroup();
            var bootstrap = new Bootstrap();
            bootstrap
                .Group(group)
                .Channel<TcpSocketChannel>()
                .Option(ChannelOption.SoTimeout, SoTimeout)
                .Option(ChannelOption.TcpNodelay, true)
                .Handler(new ActionChannelInitializer<ISocketChannel>(channel =>
                {
                    IChannelPipeline pipeline = channel.Pipeline;
                    pipeline.AddLast(new DelimiterBasedFrameDecoder(8192, Delimiters.LineDelimiter()));
                    pipeline.AddLast(new StringEncoder(), new StringDecoder(), this);
                }));
            _clientChannel = await bootstrap.ConnectAsync(new IPEndPoint(IPAddress.Parse(Address), Port)).ConfigureAwait(false);
            _clientChannel.Read();
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
                channel.WriteAsync(Unpooled.WrappedBuffer(body));
            }
        }
        private void WriteWithHeader(byte[] body)
        {
            WriteWithHeader(writableChannel, body);
        }


        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            var byteBuffer = message as IByteBuffer;
            if (byteBuffer != null)
            {
                var p = Packet.Parser.ParseFrom(byteBuffer.Array);
                if (p.Version != 1)
                {
                    throw new CanalClientException("unsupported version at this client.");
                }

                if (p.Type != PacketType.Handshake)
                {
                    throw new CanalClientException("expect handshake but found other type.");
                }
                var handshake = Handshake.Parser.ParseFrom(p.Body);
                supportedCompressions.Add(handshake.SupportedCompressions);
                var ca = new ClientAuth()
                {
                    Username = Username != null ? Username : "",
                    Password = ByteString.CopyFromUtf8(Password != null ? Password : ""),
                    NetReadTimeout = IdleTimeout,
                    NetWriteTimeout = IdleTimeout
                };
                context.WriteAndFlushAsync(new Packet()
                {
                    Type = PacketType.Clientauthentication,
                    Body = ca.ToByteString()
                }.ToByteArray());

                var ack = Packet.Parser.ParseFrom(byteBuffer.Array);
                if (ack.Type != PacketType.Ack)
                {
                    throw new CanalClientException("unexpected packet type when ack is expected");
                }


                var ackBody = Com.Alibaba.Otter.Canal.Protocol.Ack.Parser.ParseFrom(ack.Body);
                if (ackBody.ErrorCode > 0)
                {
                    throw new CanalClientException($"something goes wrong when doing authentication:{ackBody.ErrorMessage} ");
                }

                _connected = true;
            }

        }
    }


}
