using ProtoBuf;

namespace NPSharp.RPC.Messages
{
    [Packet(1011)]
    [ProtoContract]
    public sealed class AuthenticateUserGroupMessage : RPCServerMessage
    {
        [ProtoMember(1)]
        public int GroupID { get; set; }
    }
}