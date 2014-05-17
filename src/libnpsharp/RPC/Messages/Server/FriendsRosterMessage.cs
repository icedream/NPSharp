using NPSharp.RPC.Messages.Structs;
using ProtoBuf;

namespace NPSharp.RPC.Messages.Server
{
    [ProtoContract]
    [Packet(1211)]
    public sealed class FriendsRosterMessage : RPCServerMessage
    {
        internal FriendsRosterMessage()
        {
        }

        [ProtoMember(1)]
        public FriendDetails[] Friends { get; set; }
    }
}