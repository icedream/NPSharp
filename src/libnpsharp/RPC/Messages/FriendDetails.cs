using System;
using ProtoBuf;

namespace NPSharp.RPC.Messages
{
    [ProtoContract]
    public sealed class FriendDetails
    {
        internal FriendDetails()
        {
        }

        [ProtoMember(1)]
        public UInt64 NPID { get; set; }

        [ProtoMember(2)]
        public string Name { get; set; }
    }
}