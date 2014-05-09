using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net.Appender;

namespace NPSharp
{
    internal class Ticket
    {
        public Ticket(byte[] data)
        {
            if (data.Length < sizeof(uint) + (sizeof(ulong) * 2) + sizeof(uint))
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

        public uint Version { get; private set; }

        public ulong ClientID { get; private set; }

        public ulong ServerID { get; private set; }

        public uint Time { get; private set; }
    }
}
