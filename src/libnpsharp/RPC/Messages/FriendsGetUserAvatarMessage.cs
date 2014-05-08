using System;
using ProtoBuf;

namespace NPSharp.RPC.Messages
{
    [ProtoContract]
    [Packet(1214)]
    public sealed class FriendsGetUserAvatarMessage : RPCClientMessage
    {
        [ProtoMember(1)]
        public Int32 Guid { get; set; }
    }
}