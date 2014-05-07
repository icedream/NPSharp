using System;

namespace NPSharp.Rpc.Packets
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
