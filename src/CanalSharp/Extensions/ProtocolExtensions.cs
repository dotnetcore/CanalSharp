using System;
using System.IO;
using System.Threading.Tasks;
using BeetleX;
using BeetleX.Clients;
using CanalSharp.Protocol;

namespace CanalSharp
{
    public static class ProtocolExtensions
    {
        /// <summary>
        /// Validate Canal ack packet.
        /// </summary>
        /// <param name="p"></param>
        public static void ValidateAck(this Packet p)
        {
            if (p.Type != PacketType.Ack)
            {
                throw new CanalConnectionException("Unexpected packet type when ack is expected.");
            }

            var ackBody = Ack.Parser.ParseFrom(p.Body);
            if (ackBody.ErrorCode > 0)
            {
                throw new CanalConnectionException("Received an error returned by the server: " +
                                                        ackBody.ErrorMessage);
            }
        }

        static readonly byte[] HeaderBuffer = new byte[4];

        /// <summary>
        /// Read Canal packet.
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public static async Task<Packet> ReadPacketAsync(this TcpClient client)
        {
            var reader = client.Receive();
            await reader.ReadAsync(HeaderBuffer);
            Array.Reverse(HeaderBuffer);
            var bodyLength = BitConverter.ToInt32(HeaderBuffer);

            while (reader.Length < bodyLength)
            {
                reader = client.Receive();
            }
            var bodyByte = reader.ReadBytes(bodyLength);
            var p = Packet.Parser.ParseFrom(bodyByte.Data, bodyByte.Offset, bodyLength);
            bodyByte.Dispose();

            return p;
        }

        /// <summary>
        /// Write Canal packet.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static async Task WritePacketAsync(this TcpClient client, byte[] data)
        {
            var headerBytes = BitConverter.GetBytes(data.Length);
            Array.Reverse(headerBytes);
            var writer = client.Stream.ToPipeStream();
            await writer.WriteAsync(headerBytes);
            await writer.WriteAsync(data);
            await writer.FlushAsync();
        }
    }
}