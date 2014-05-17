using System;
using ProtoBuf;

namespace NPSharp.RPC.Messages.Structs
{
    [ProtoContract]
    public sealed class ProfileData
    {
        internal ProfileData()
        {
        }

        [ProtoMember(1)]
        public UInt64 NPID { get; set; }

        [ProtoMember(2)]
        public byte[] Profile { get; set; }
    }
}