using ProtoBuf;

namespace NPSharp.RPC.Packets
{
    [Packet(1006)]
    [ProtoContract]
    class AuthenticateExternalStatusMessage : RPCServerMessage
    {
        [ProtoMember(1)]
        public int Status { get; set; }
    }
}