using ProtoBuf;

namespace NPSharp.RPC.Messages
{
    [Packet(1006)]
    [ProtoContract]
    public sealed class AuthenticateExternalStatusMessage : RPCServerMessage
    {
        [ProtoMember(1)]
        public int Status { get; set; }
    }
}