using ProtoBuf;

namespace NPSharp.RPC.Messages
{
    [Packet(1022)]
    [ProtoContract]
    public sealed class AuthenticateRegisterServerResultMessage : RPCServerMessage
    {
        [ProtoMember(1)]
        public int Result { get; set; }

        [ProtoMember(2)]
        public string LicenseKey { get; set; }

        [ProtoMember(3)]
        public int ServerID { get; set; }
    }
}