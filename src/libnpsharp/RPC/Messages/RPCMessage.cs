using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using log4net;
using ProtoBuf;

namespace NPSharp.RPC.Messages
{
    public abstract class RPCMessage
    {
        internal const uint Signature = 0xDEADC0DE;
        // I wonder if aiw3 changed this since kernal noted it in his source code.

        private static readonly ILog Log;

        static RPCMessage()
        {
            Log = LogManager.GetLogger("RPC");
        }

        internal uint MessageId { get; set; }

        public uint GetTypeId()
        {
            var packet = (PacketAttribute) GetType().GetCustomAttributes(typeof (PacketAttribute), false).Single();
            return packet.Type;
        }

        public static T Deserialize<T>(Socket sock) where T : RPCMessage
        {
            var header = new byte[4*sizeof (uint)];

            while (sock.Connected && !sock.Poll(1000, SelectMode.SelectRead))
            {
            }

            if (!sock.Connected)
            {
                // Socket disconnected
                return null;
            }

            try
            {
                var l = sock.Receive(header);
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
            }
            catch (SocketException)
            {
                if (sock.Connected)
                    throw;
                return null;
            }
#if !DEBUG
            catch (Exception error)
            {
                Log.Error("Error while reading from network socket", error)
                return null;
            }
#endif

            uint signature, length, type, mid;
            using (var ms = new MemoryStream(header))
            {
                using (var br = new BinaryReader(ms))
                {
                    signature = br.ReadUInt32();
                    length = br.ReadUInt32();
                    type = br.ReadUInt32();
                    mid = br.ReadUInt32();
                }
            }

            var buffer = new byte[length];
            sock.Receive(buffer);

            if (signature != Signature)
            {
                Log.Error("Received invalid signature");
                throw new ProtocolViolationException("Received packet with invalid signature");
            }

            T message;

            using (var ms = new MemoryStream(buffer))
            {
                Type[] types = Assembly.GetExecutingAssembly().GetTypes().Where(
                    t =>
                        t.IsSubclassOf(typeof (T))
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
                message = (T) Serializer.NonGeneric.Deserialize(
                    types.Single(),
                    ms
                    );
            }

            message.MessageId = mid;

#if DEBUG
            Log.DebugFormat("{3}[ID={0},Type={1},TypeName={2}] {{", mid, message.GetTypeId(), message.GetType().Name,
                typeof (T).Name);
            foreach (
                PropertyInfo prop in
                    message.GetType().GetProperties().Where(p => !(p.DeclaringType == typeof (RPCServerMessage))))
            {
                Log.DebugFormat("\t{0} = {1}", prop.Name, prop.GetValue(message));
            }
            Log.DebugFormat("}} // deserialized from {0} bytes", header.Length + buffer.Length);
#endif

            return message;
        }

        public byte[] Serialize()
        {
            byte[] content;
            using (var bufferStream = new MemoryStream())
            {
                Serializer.Serialize(bufferStream, this);
                bufferStream.Seek(0, SeekOrigin.Begin);
                content = bufferStream.ToArray();
            }

            byte[] buffArray;
            using (var ms = new MemoryStream())
            {
                using (var bw = new BinaryWriter(ms))
                {
                    bw.Write(Signature);
                    bw.Write((uint) content.Length);
                    bw.Write(GetTypeId());
                    bw.Write(MessageId);
                    bw.Write(content);
                    bw.Flush();

                    ms.Seek(0, SeekOrigin.Begin);
                    buffArray = ms.ToArray();
                }
            }

#if DEBUG
            Log.DebugFormat("{3}[ID={0},Type={1},TypeName={2}] {{", MessageId, GetTypeId(), GetType().Name,
                GetType().Name);
            foreach (PropertyInfo prop in GetType().GetProperties())
            {
                Log.DebugFormat("\t{0} = {1}", prop.Name, prop.GetValue(this));
            }
            Log.DebugFormat("}} // serialized to {0} bytes", buffArray.Length);
#endif

            return buffArray;
        }
    }
}