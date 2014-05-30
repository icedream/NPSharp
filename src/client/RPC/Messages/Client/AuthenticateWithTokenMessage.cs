using ProtoBuf;

namespace NPSharp.RPC.Messages.Client
{
    [Packet(1003)]
    [ProtoContract]
    public sealed class AuthenticateWithTokenMessage : RPCClientMessage
    {
        [ProtoMember(1)]
        public string Token { get; set; }
    }
}