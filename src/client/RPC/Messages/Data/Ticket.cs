using System;
using System.IO;

namespace NPSharp.RPC.Messages.Data
{
    internal class Ticket
    {
        public Ticket(byte[] data)
        {
            if (data.Length < sizeof (uint) + (sizeof (ulong)*2) + sizeof (uint))
            {
                throw new ArgumentException("Data buffer too short");
            }

            using (var ms = new MemoryStream(data))
            using (var br = new BinaryReader(ms))
            {
                Version = br.ReadUInt32();
                ClientID = br.ReadUInt64();
                ServerID = br.ReadUInt64();
                Time = br.ReadUInt32();
            }
        }

        // TODO: Maybe leave out arguments which are supposed to be autofilled
        public Ticket(uint version, ulong clientID, ulong serverID, uint? time = null)
        {
            Version = version;
            ClientID = clientID;
            ServerID = serverID;
            if (time.HasValue)
                Time = time.Value;
            else
                Time = (uint) DateTime.Now.ToUniversalTime().ToBinary();
        }

        public uint Version { get; private set; }

        public ulong ClientID { get; private set; }

        public ulong ServerID { get; private set; }

        public uint Time { get; private set; }

        public byte[] Serialize()
        {
            using (var ms = new MemoryStream(sizeof (uint) + (sizeof (ulong)*2) + sizeof (uint)))
            {
                using (var bw = new BinaryWriter(ms))
                {
                    bw.Write(Version);
                    bw.Write(ClientID);
                    bw.Write(ServerID);
                    bw.Write(Time);
                    ms.Seek(0, SeekOrigin.Begin);
                    return ms.ToArray();
                }
            }
        }
    }
}