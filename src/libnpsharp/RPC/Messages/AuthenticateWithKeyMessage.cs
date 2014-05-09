using ProtoBuf;

namespace NPSharp.RPC.Messages
{
    [Packet(1001)]
    [ProtoContract]
    public sealed class AuthenticateWithKeyMessage : RPCClientMessage
    {
        [ProtoMember(1)]
        public string LicenseKey { get; set; }
    }
}