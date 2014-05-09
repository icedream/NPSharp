using System;
using ProtoBuf;

namespace NPSharp.RPC.Messages
{
    [ProtoContract]
    [Packet(1202)]
    public sealed class FriendsGetProfileDataMessage : RPCClientMessage
    {
        [ProtoMember(1)]
        public UInt64[] FriendIDs { get; set; }

        [ProtoMember(2)]
        public string ProfileType { get; set; }
    }
}