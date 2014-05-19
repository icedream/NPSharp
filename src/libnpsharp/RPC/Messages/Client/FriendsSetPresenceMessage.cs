using NPSharp.RPC.Messages.Data;
using ProtoBuf;

namespace NPSharp.RPC.Messages.Client
{
    [ProtoContract]
    [Packet(1213)]
    public sealed class FriendsSetPresenceMessage : RPCClientMessage
    {
        [ProtoMember(1)]
        public FriendsPresence[] Presence { get; set; }
    }
}