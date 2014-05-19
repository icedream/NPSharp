using NPSharp.RPC.Messages.Data;
using ProtoBuf;

namespace NPSharp.RPC.Messages.Server
{
    [ProtoContract]
    [Packet(1203)]
    public sealed class FriendsGetProfileDataResultMessage : RPCServerMessage
    {
        [ProtoMember(1)]
        public ProfileData[] Results { get; set; }
    }
}