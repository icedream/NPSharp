using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;

namespace NPSharp.RPC.Packets
{
    public class RPCServerMessage : RPCMessage
    {
        public uint MessageId { get; private set; }

        public static RPCServerMessage Deserialize(NetworkStream ns)
        {
            var header = new byte[16];
            var l = ns.Read(header, 0, header.Length);
            if (l == 0)
                return null;
            if (l < 16)
                 throw new ProtocolViolationException("Received incomplete header");

            var signature = (uint)IPAddress.NetworkToHostOrder(BitConverter.ToUInt32(header, 0));
            var length = (uint)IPAddress.NetworkToHostOrder(BitConverter.ToUInt32(header, 4));
            var type = (uint)IPAddress.NetworkToHostOrder(BitConverter.ToUInt32(header, 8));
            var buffer = new byte[length];
            ns.Read(buffer, 0, buffer.Length);

            if (signature != Signature)
                throw new ProtocolViolationException("Received packet with invalid signature");

            RPCServerMessage packet;

            using (var ms = new MemoryStream(buffer))
            {
                var types = Assembly.GetExecutingAssembly().GetTypes().Where(
                    t =>
                        t.IsSubclassOf(typeof (RPCServerMessage))
                        &&
                        ((PacketAttribute) t.GetCustomAttributes(typeof (PacketAttribute), false).Single()).Type == type
                    ).ToArray();
                if (!types.Any())
                {
                    throw new ProtocolViolationException("Received packet of unknown type");
                }
                if (types.Count() > 1)
                {
#if DEBUG
                    Debug.Fail(string.Format("Bug in program code: Found more than 1 type for packet ID {0}", type));
#else
                    // TODO: log4net
                    return null;
#endif
                }
                packet = (RPCServerMessage)ProtoBuf.Serializer.NonGeneric.Deserialize(
                        types.Single(),
                        ms
                    );
            }

            packet.MessageId = (uint)IPAddress.NetworkToHostOrder(BitConverter.ToUInt32(header, 12));

            return packet;
        }
    }
}