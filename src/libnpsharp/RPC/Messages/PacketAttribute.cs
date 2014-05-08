using System;

namespace NPSharp.RPC.Messages
{
    internal sealed class PacketAttribute : Attribute
    {
        public PacketAttribute(uint type)
        {
            Type = type;
        }

        public uint Type { get; set; }
    }
}
