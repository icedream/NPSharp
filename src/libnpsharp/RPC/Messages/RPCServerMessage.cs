using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using log4net;

namespace NPSharp.RPC.Messages
{
    public abstract class RPCServerMessage : RPCMessage
    {

        private static readonly ILog Log;

        static RPCServerMessage()
        {
            Log = LogManager.GetLogger("RPCServerMessage");
        }

        // Internal constructor to make classes unconstructible from outside
        internal RPCServerMessage() { }

        public uint MessageId { get; private set; }

        public static RPCServerMessage Deserialize(NetworkStream ns)
        {
            var header = new byte[4 * sizeof(uint)];
            Log.Debug("Reading...");
            var l = ns.Read(header, 0, header.Length);
            if (l == 0)
            {
                Log.Debug("Received 0 bytes");
                return null;
            }
            if (l < 16)
            {
                Log.ErrorFormat("Received incomplete header ({0} bytes of 16 wanted bytes)", l);
                throw new ProtocolViolationException("Received incomplete header");
            }

            uint signature, length, type, pid;
            using (var ms = new MemoryStream(header))
            {
                using (var br = new BinaryReader(ms))
                {
                    signature = br.ReadUInt32();
                    length = br.ReadUInt32();
                    type = br.ReadUInt32();
                    pid = br.ReadUInt32();
                }
            }

            var buffer = new byte[length];
            ns.Read(buffer, 0, buffer.Length);

            if (signature != Signature)
            {
                Log.Error("Received invalid signature");
                throw new ProtocolViolationException("Received packet with invalid signature");
            }

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
                    throw new ProtocolViolationException(string.Format("Received packet of unknown type ({0})", type));
                }
                if (types.Count() > 1)
                {
#if DEBUG
                    Debug.Fail(string.Format("Bug in program code: Found more than 1 type for packet ID {0}", type));
#else
                    return null;
#endif
                }
                packet = (RPCServerMessage)ProtoBuf.Serializer.NonGeneric.Deserialize(
                        types.Single(),
                        ms
                    );
            }

            packet.MessageId = pid;

#if DEBUG
            Log.DebugFormat("ServerMessage[ID={0},Type={1},TypeName={2}] {{", pid, packet.GetTypeId(), packet.GetType().Name);
            foreach (var prop in packet.GetType().GetProperties())
            {
                Log.DebugFormat("\t{0} = {1}", prop.Name, prop.GetValue(packet));
            }
            Log.DebugFormat("}} => Read from {0} bytes", header.Length + buffer.Length);
#endif

            return packet;
        }
    }
}