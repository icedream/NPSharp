using ProtoBuf;

namespace NPSharp.RPC.Messages
{
    [Packet(1004)]
    [ProtoContract]
    public sealed class AuthenticateRegisterServerMessage : RPCClientMessage
    {
        [ProtoMember(1, IsRequired = false)]
        public string ConfigPath { get; set; }
    }
}