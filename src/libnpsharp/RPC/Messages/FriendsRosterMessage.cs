using ProtoBuf;

namespace NPSharp.RPC.Messages
{
    [ProtoContract]
    [Packet(1211)]
    public sealed class FriendsRosterMessage : RPCServerMessage
    {
        internal FriendsRosterMessage() { }

        [ProtoMember(1)]
        public FriendDetails[] Friends { get; set; }
    }
}