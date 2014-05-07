using ProtoBuf;

namespace NPSharp.Rpc.Packets
{
    [Packet(1006)]
    [ProtoContract]
    class AuthenticateExternalStatusMessage : RpcServerMessage
    {
        [ProtoMember(1)]
        public int Status { get; set; }
    }
}