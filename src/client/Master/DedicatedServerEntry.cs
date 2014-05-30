using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace NPSharp.Master
{
    public class DedicatedServerEntry
    {
        internal DedicatedServerEntry(IPAddress ip, params ushort[] ports)
        {
            IP = ip;
            Ports = ports;
        }

        public IPAddress IP { get; private set; }

        public ushort[] Ports { get; private set; }

        internal byte[] Serialize(bool standardFormat = true)
        {
            if (standardFormat && IP.AddressFamily != AddressFamily.InterNetwork)
                throw new InvalidOperationException("Can't serialize non-IPv4 addresses into standard format");

            var buffer = new List<byte>();

            if (standardFormat)
                buffer.AddRange(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(IP.GetHashCode()))); // TODO: GetHashCode == IP address as number???
            else
            {
                // TODO: Implement extended serialization format for IP addresses!
                throw new NotImplementedException("Extended serialization format not implemented yet");
            }

            foreach (var port in Ports)
                buffer.AddRange(BitConverter.GetBytes((ushort) IPAddress.HostToNetworkOrder((short) port)));

            return buffer.ToArray();
        }

        // TODO: Deserialize
    }
}