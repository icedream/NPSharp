using ProtoBuf;

namespace NPSharp.RPC.Messages
{
    [Packet(1002)]
    [ProtoContract]
    public sealed class AuthenticateWithDetailsMessage : RPCClientMessage
    {
        [ProtoMember(1)]
        public string Username { get; set; }

        [ProtoMember(2)]
        public string Password { get; set; }
    }
}