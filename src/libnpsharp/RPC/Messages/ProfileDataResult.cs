using System;
using ProtoBuf;

namespace NPSharp.RPC.Messages
{
    [ProtoContract]
    public sealed class ProfileDataResult
    {
        internal ProfileDataResult() { }

        [ProtoMember(1)]
        public UInt64 NPID { get; set; }

        [ProtoMember(2)]
        public byte[] Profile { get; set; }
    }
}