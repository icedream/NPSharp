using ProtoBuf;

namespace NPSharp.RPC.Messages
{
    [ProtoContract]
    [Packet(1203)]
    public sealed class FriendsGetProfileDataResultMessage : RPCServerMessage
    {
        [ProtoMember(1)]
        public ProfileDataResult[] Results { get; set; }
    }
}