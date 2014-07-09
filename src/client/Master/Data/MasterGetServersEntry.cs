using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NPSharp.Master.Data
{
    public class MasterGetServersEntry
    {
        internal MasterGetServersEntry(byte[] data)
        {
            if (data.Length < 4 || data.Length % 2 > 0)
                throw new ArgumentException("Data length must be at least 4 bytes of IP address and can afterwards only contain full 2 byte segments of ushort port data.");

            IP = new IPAddress(data.Take(4).ToArray());
            Ports = new ushort[(data.Length - 4) % sizeof(ushort)];
            for (var i = 4; i < data.Length; i += sizeof(ushort))
            {
                Ports[(i - 4)/2] = (ushort) IPAddress.NetworkToHostOrder((short) BitConverter.ToUInt16(data, i));
            }
        }

        internal MasterGetServersEntry(IPAddress ip, ushort[] ports)
        {
            IP = ip;
            Ports = ports;
        }

        public IPAddress IP { get; private set; }

        public ushort[] Ports { get; set; }
    }
}
