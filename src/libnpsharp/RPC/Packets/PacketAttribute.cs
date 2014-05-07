using System;

namespace NPSharp.RPC.Packets
{
    class PacketAttribute : Attribute
    {
        public PacketAttribute(uint type)
        {
            Type = type;
        }

        public uint Type { get; set; }
    }
}
