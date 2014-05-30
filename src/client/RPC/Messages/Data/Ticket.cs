using System;
using System.IO;

namespace NPSharp.RPC.Messages.Data
{
    /// <summary>
    /// Represents a ticket which is used to validate client-to-gameserver connections.
    /// </summary>
    public class Ticket
    {
        /// <summary>
        /// Reconstructs the ticket from raw byte data.
        /// </summary>
        /// <param name="data">The ticket's raw data</param>
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

        /// <summary>
        /// Constructs a ticket from given parameters.
        /// </summary>
        /// <param name="clientID">The client NPID</param>
        /// <param name="serverID">The server NPID</param>
        /// <param name="version">The ticket's structure version</param>
        /// <param name="time">The ticket time</param>
        public Ticket(ulong clientID, ulong serverID, uint version = 1, uint? time = null)
        {
            ClientID = clientID;
            ServerID = serverID;
            Version = version;
            if (time.HasValue)
                Time = time.Value;
            else
                Time = (uint) DateTime.Now.ToUniversalTime().ToBinary();
        }

        /// <summary>
        /// Ticket structure version.
        /// </summary>
        public uint Version { get; private set; }

        /// <summary>
        /// The client's ID on the NP server.
        /// </summary>
        public ulong ClientID { get; private set; }

        /// <summary>
        /// The gameserver's ID on the NP server.
        /// </summary>
        public ulong ServerID { get; private set; }

        /// <summary>
        /// The creation time of the ticket.
        /// </summary>
        public uint Time { get; private set; }

        internal byte[] Serialize()
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