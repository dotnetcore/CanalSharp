// Licensed to the Apache Software Foundation (ASF) under one
// or more contributor license agreements.  See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership.  The ASF licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License.  You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using CanalSharp.Common.Logging;
using CanalSharp.Protocol;
using CanalSharp.Protocol.Exception;
using Com.Alibaba.Otter.Canal.Protocol;
using DotNetty.Transport.Channels;
using Google.Protobuf;
using Microsoft.Extensions.Logging;

namespace CanalSharp.Client.Impl
{
    public class SimpleCanalConnector : ChannelHandlerAdapter, ICanalConnector
    {
        private readonly ILogger _logger = CanalSharpLogManager.LoggerFactory.CreateLogger<SimpleCanalConnector>();

        private readonly ClientIdentity _clientIdentity;

        /// <summary>
        ///  To indicate whether the connector has been executed correctly.
        /// Because of HA, it dose not mean at work.
        /// </summary>
        private volatile bool _connected;

        private volatile bool _running;

        /// <summary>
        /// To recode the value submitted by the last filter, which is easy to submit when automatically retrying.
        /// </summary>
        private string _filter;

        private List<Compression> _supportedCompressions = new List<Compression>();
        private static readonly object _writeDataLock = new object();

        private static readonly object _readDataLock = new object();

        // Whether the rollback operation is performed automatically after the connector is successfully connected.
        private bool _rollbackOnConnect = true;

        //        private Message _message;

        // Whether to automatically parse the Entry object. If you consider maximzing performance, you can delay parsing.
        private bool _lazyParseEntry = false;
        private TcpClient _tcpClient;
        private NetworkStream _channelNetworkStream;

        /// <summary>
        /// Whether to automatically run a rollback operation after the connection is successful.
        /// </summary>
        private bool _rollbackOnDisConnect = false;

        public string Address { get; set; }

        public int Port { get; set; }

        public string UserName { get; set; }

        public string PassWord { get; set; }

        /// <summary>
        ///  // milliseconds
        /// </summary>
        public int SoTimeOut { get; set; } = 60000;

        /// <summary>
        /// The timeout period for idle connections between client and server, default 1 hour.
        /// </summary>
        public int IdleTimeOut { get; set; } = 60 * 60 * 1000;

        public SimpleCanalConnector(string address, int port, string username, string password, string destination) :
            this(address, port, username, password, destination, 60000, 60 * 60 * 1000)
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
            UserName = username;
            UserName = password;
            SoTimeOut = soTimeout;
            IdleTimeOut = idleTimeout;
            _clientIdentity = new ClientIdentity(destination, (short)1001);
        }

        public void Connect()
        {
            if (_connected)
            {
                return;
            }

            WaitClientRunning();
            if (!_running)
            {
                return;
            }

            DoConnect();
            if (_filter != null)
            {
                // 如果存在条件，说明是自动切换，基于上一次的条件订阅一次
                Subscribe(_filter);
            }

            if (_rollbackOnConnect)
            {
                Rollback();
            }

            _connected = true;
        }

        public void Disconnect()
        {
            if (_rollbackOnDisConnect && _tcpClient.Connected == false)
            {
                Rollback();
            }

            _connected = false;
            DoDisConnection();
        }

        private void DoDisConnection()
        {
            if (_tcpClient != null)
            {
                QuietlyClose();
            }
        }

        private void QuietlyClose()
        {
            _tcpClient.Close();
        }

        public bool CheckValid()
        {
            throw new NotImplementedException();
        }

        public void Subscribe(string filter)
        {
            WaitClientRunning();
            if (!_running)
            {
                return;
            }

            try
            {
                var sub = new Sub()
                {
                    Destination = _clientIdentity.Destination,
                    ClientId = _clientIdentity.ClientId.ToString(),
                    Filter = string.IsNullOrEmpty(filter) ? ".*\\..*" : filter
                };
                var pack = new Packet()
                {
                    Type = PacketType.Subscription,
                    Body = sub.ToByteString()
                }.ToByteArray();

                WriteWithHeader(pack);

                var p = Packet.Parser.ParseFrom(ReadNextPacket());
                var ack = Com.Alibaba.Otter.Canal.Protocol.Ack.Parser.ParseFrom(p.Body);
                if (ack.ErrorCode > 0)
                {
                    throw new CanalClientException($"failed to subscribe with reason: {ack.ErrorMessage}");
                }

                _clientIdentity.Filter = filter;
                _filter = filter;
                _logger.LogDebug($"Subscribe success. Filter: {filter}");
            }
            catch (Exception e)
            {
                throw new CanalClientException(e.Message);
            }
        }

        public void Subscribe()
        {
            Subscribe("");
        }

        public void UnSubscribe()
        {
            WaitClientRunning();
            if (!_running)
            {
                return;
            }

            try
            {
                var unsub = new Unsub()
                {
                    Destination = _clientIdentity.Destination,
                    ClientId = _clientIdentity.ClientId.ToString(),
                };
                var pack = new Packet()
                {
                    Type = PacketType.Unsubscription,
                    Body = unsub.ToByteString()
                }.ToByteArray();
                WriteWithHeader(pack);
                var p = Packet.Parser.ParseFrom(ReadNextPacket());
                var ack = Com.Alibaba.Otter.Canal.Protocol.Ack.Parser.ParseFrom(p.Body);
                if (ack.ErrorCode > 0)
                {
                    throw new CanalClientException($"failed to unSubscribe with reason: {ack.ErrorMessage}");
                }
            }
            catch (IOException e)
            {
                throw new CanalClientException(e.Message, e);
            }
        }

        public Message Get(int batchSize)
        {
            var message = Get(batchSize, null, null);
            return message;
        }

        public Message Get(int batchSize, long? timeout, int? unit)
        {
            var message = GetWithoutAck(batchSize, timeout, unit);
            Ack(message.Id);
            return message;
        }

        public Message GetWithoutAck(int batchSize)
        {
            return GetWithoutAck(batchSize, null, null);
        }

        public Message GetWithoutAck(int batchSize, long? timeout, int? unit)
        {
            WaitClientRunning();
            if (!_running)
            {
                return null;
            }

            var size = (batchSize <= 0) ? 1000 : batchSize;
            // -1 代表不做 timeout 控制
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

            WriteWithHeader(packet);

            return ReceiveMessages();
        }

        private Message ReceiveMessages()
        {
            var data = ReadNextPacket();
            var p = Packet.Parser.ParseFrom(data);
            switch (p.Type)
            {
                case PacketType.Messages:
                    {
                        if (!p.Compression.Equals(Compression.None))
                        {
                            throw new CanalClientException("compression is not supported in this connector");
                        }

                        var messages = Messages.Parser.ParseFrom(p.Body);
                        var result = new Message(messages.BatchId);
                        if (_lazyParseEntry)
                        {
                            // byteString
                            result.RawEntries = messages.Messages_.ToList();
                        }
                        else
                        {
                            foreach (var byteString in messages.Messages_)
                            {
                                result.Entries.Add(Entry.Parser.ParseFrom(byteString));
                            }
                        }

                        return result;
                    }
                case PacketType.Ack:
                    {
                        var ack = Com.Alibaba.Otter.Canal.Protocol.Ack.Parser.ParseFrom(p.Body);
                        throw new CanalClientException($"something goes wrong with reason:{ack.ErrorMessage}");
                    }
                default:
                    {
                        throw new CanalClientException($"unexpected packet type: {p.Type}");
                    }
            }
        }

        public void Ack(long batchId)
        {
            WaitClientRunning();
            if (!_running)
            {
                return;
            }

            var ca = new ClientAck()
            {
                Destination = _clientIdentity.Destination,
                ClientId = _clientIdentity.ClientId.ToString(),
                BatchId = batchId
            };

            var pack = new Packet()
            {
                Type = PacketType.Clientack,
                Body = ca.ToByteString()
            }.ToByteArray();

            try
            {
                WriteWithHeader(pack);
            }
            catch (IOException e)
            {
                throw new CanalClientException(e.Message, e);
            }
        }

        public void Rollback(long batchId)
        {
            WaitClientRunning();

            var ca = new ClientRollback()
            {
                Destination = _clientIdentity.Destination,
                ClientId = _clientIdentity.ClientId.ToString(),
                BatchId = batchId
            };

            try
            {
                var pack = new Packet()
                {
                    Type = PacketType.Clientrollback,
                    Body = ca.ToByteString()
                }.ToByteArray();

                WriteWithHeader(pack);
            }
            catch (IOException e)
            {
                throw new CanalClientException(e.Message, e);
            }
        }

        public void Rollback()
        {
            WaitClientRunning();
            Rollback(0); // 0 代笔未设置
        }

        public void StopRunning()
        {
            throw new NotImplementedException();
        }

        private void DoConnect()
        {
            _tcpClient = new TcpClient(Address, Port);
            _channelNetworkStream = _tcpClient.GetStream();
            var p = Packet.Parser.ParseFrom(ReadNextPacket());
            if (p != null)
            {
                if (p.Version != 1)
                {
                    throw new CanalClientException("unsupported version at this client.");
                }

                if (p.Type != PacketType.Handshake)
                {
                    throw new CanalClientException("expect handshake but found other type.");
                }

                var handshake = Handshake.Parser.ParseFrom(p.Body);
                _supportedCompressions.Add(handshake.SupportedCompressions);

                var ca = new ClientAuth()
                {
                    Username = UserName ?? "",
                    Password = ByteString.CopyFromUtf8(PassWord ?? ""),
                    NetReadTimeout = IdleTimeOut,
                    NetWriteTimeout = IdleTimeOut
                };

                var packArray = new Packet()
                {
                    Type = PacketType.Clientauthentication,
                    Body = ca.ToByteString()
                }.ToByteArray();

                WriteWithHeader(packArray);

                var packet = Packet.Parser.ParseFrom(ReadNextPacket());
                if (packet.Type != PacketType.Ack)
                {
                    throw new CanalClientException("unexpected packet type when ack is expected");
                }

                var ackBody = Com.Alibaba.Otter.Canal.Protocol.Ack.Parser.ParseFrom(p.Body);
                if (ackBody.ErrorCode > 0)
                {
                    throw new CanalClientException("something goes wrong when doing authentication:" +
                                                   ackBody.ErrorMessage);
                }

                _connected = _tcpClient.Connected;
                _logger.LogDebug($"Canal connect success. IP: {Address}, Port: {Port}");
            }
        }


        private byte[] ReadNextPacket()
        {
            lock (_readDataLock)
            {
                var headerLength = ReadHeaderLength();
                var receiveData = new byte[1024 * 2];
                using (var ms = new MemoryStream())
                {
                    while (headerLength > 0)
                    {
                        var len = _channelNetworkStream.Read(receiveData, 0,
                            (headerLength > receiveData.Length ? receiveData.Length : headerLength));
                        ms.Write(receiveData, 0, len);
                        headerLength = headerLength - len;
                    }

                    return ms.ToArray();
                }
            }
        }

        private int ReadHeaderLength()
        {
            var headerBytes = new byte[4];
            _channelNetworkStream.Read(headerBytes, 0, 4);
            Array.Reverse(headerBytes);
            return BitConverter.ToInt32(headerBytes, 0);
        }

        private void WriteWithHeader(byte[] body)
        {
            lock (_writeDataLock)
            {
                var len = body.Length;
                var bytes = GetHeaderBytes(len);
                _channelNetworkStream.Write(bytes, 0, bytes.Length);
                _channelNetworkStream.Write(body, 0, body.Length);
            }
        }


        private byte[] GetHeaderBytes(int lenth)
        {
            var data = BitConverter.GetBytes(lenth);
            Array.Reverse(data);
            return data;
        }

        private void WaitClientRunning()
        {
            _running = true;
        }
    }
}