using ProtoBuf;

namespace NPSharp.RPC.Messages.Server
{
    [Packet(1006)]
    [ProtoContract]
    public sealed class AuthenticateExternalStatusMessage : RPCServerMessage
    {
        [ProtoMember(1)]
        public int Status { get; set; }
    }
}