using System;
using System.IO;
using log4net;

namespace NPSharp.RPC.Messages
{
    public abstract class RPCClientMessage : RPCMessage
    {

        private static readonly ILog Log;

        static RPCClientMessage()
        {
            Log = LogManager.GetLogger("RPCClientMessage");
        }

        public byte[] Serialize(uint id)
        {
#if DEBUG
            Log.DebugFormat("Packet[ID={0},Type={1},TypeName={2}] {{", id, GetTypeId(), GetType().Name);
            foreach (var prop in GetType().GetProperties())
            {
                Log.DebugFormat("\t{0} = {1}", prop.Name, prop.GetValue(this));
            }
#endif

            byte[] content;
            using (var bufferStream = new MemoryStream())
            {
                ProtoBuf.Serializer.Serialize(bufferStream, this);
                bufferStream.Seek(0, SeekOrigin.Begin);
                content = bufferStream.ToArray();
            }

            Log.DebugFormat("}} => Serialized to {0} bytes", content.Length);

            byte[] buffArray;
            using (var ms = new MemoryStream())
            {
                using (var bw = new BinaryWriter(ms))
                {
                    bw.Write(Signature);
                    bw.Write((uint)content.Length);
                    bw.Write(GetTypeId());
                    bw.Write(id);
                    bw.Write(content);
                    bw.Flush();

                    ms.Seek(0, SeekOrigin.Begin);
                    buffArray = ms.ToArray();
                }
            }

#if DEBUG
            Console.Write("\t");

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write(BitConverter.ToString(buffArray, 0, sizeof(uint)).Replace("-", ""));

            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.Write(BitConverter.ToString(buffArray, 1 * sizeof(uint), sizeof(uint)).Replace("-", ""));

            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(BitConverter.ToString(buffArray, 2 * sizeof(uint), sizeof(uint)).Replace("-", ""));

            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(BitConverter.ToString(buffArray, 3 * sizeof(uint), sizeof(uint)).Replace("-", ""));

            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Write(BitConverter.ToString(buffArray, 4 * sizeof(uint)).Replace("-", ""));

            Console.ResetColor();
            Console.WriteLine();
#endif

            return buffArray;
        }
    }
}