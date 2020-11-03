using System.Threading.Tasks;
using BeetleX;
using BeetleX.Clients;
using CanalSharp.Protocol;
using Google.Protobuf;
using Xunit;

namespace CanalSharp.UnitTest.Protocol
{
    public class Protocol_Tests
    {
        private static TcpClient _client;
        private static void InitClient()
        {
            _client= SocketFactory.CreateClient<TcpClient>(Settings.IpAddress, Settings.Port);
        }

        [Fact]
        public async Task HandShake_ShouldBe_Success()
        {
            InitClient();
            var p = await _client.ReadPacketAsync();

            Assert.Equal(PacketType.Handshake,p.Type);
            Assert.Equal(1,p.Version);
        }

        [Fact]
        public async Task Auth_ShouldBe_Success()
        {
            InitClient();

            await HandShake_ShouldBe_Success();

            var data = new ClientAuth()
            {
                Username = Settings.UserName,
                Password = ByteString.CopyFromUtf8(Settings.Password),
                NetReadTimeout = 60 * 60 * 1000,
                NetWriteTimeout = 60 * 60 * 1000
            };

            var packet = new Packet()
            {
                Type = PacketType.Clientauthentication,
                Body = data.ToByteString()
            }.ToByteArray();

            await _client.WritePacketAsync(packet);
            var p = await _client.ReadPacketAsync();
            p.ValidateAck();
        }

        [Fact]
        public async Task Subscribe_ShouldBe_Success()
        {
            InitClient();
            await HandShake_ShouldBe_Success();
            await Auth_ShouldBe_Success();

            var data = new Sub()
            {
                Destination = Settings.Destination,
                ClientId = Settings.ClientId,
                Filter = string.IsNullOrEmpty(Settings.Filter) ? ".*\\..*" : Settings.Filter
            };

            var pack = new Packet()
            {
                Type = PacketType.Subscription,
                Body = data.ToByteString()
            }.ToByteArray();

            await _client.WritePacketAsync(pack);
            var p = await _client.ReadPacketAsync();
            p.ValidateAck();
        }

        [Fact]
        public async Task UnSubscribe_ShouldBe_Success()
        {
            InitClient();
            await HandShake_ShouldBe_Success();
            await Auth_ShouldBe_Success();
            await Subscribe_ShouldBe_Success();

            var data = new Unsub()
            {
                Destination = Settings.Destination,
                ClientId = Settings.ClientId,
                Filter = string.IsNullOrEmpty(Settings.Filter) ? ".*\\..*" : Settings.Filter
            };

            var pack = new Packet()
            {
                Type = PacketType.Subscription,
                Body = data.ToByteString()
            }.ToByteArray();

            await _client.WritePacketAsync(pack);
            var p = await _client.ReadPacketAsync();
            p.ValidateAck();
        }
    }
}