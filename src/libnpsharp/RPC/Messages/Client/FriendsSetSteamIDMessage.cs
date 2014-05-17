using System;
using ProtoBuf;

namespace NPSharp.RPC.Messages.Client
{
    [ProtoContract]
    [Packet(1201)]
    public sealed class FriendsSetSteamIDMessage : RPCClientMessage
    {
        [ProtoMember(1)]
        public UInt64 SteamID { get; set; }
    }
}