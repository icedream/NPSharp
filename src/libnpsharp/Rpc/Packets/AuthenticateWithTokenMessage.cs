using ProtoBuf;

namespace NPSharp.RPC.Packets
{
    [Packet(1003)]
    [ProtoContract]
    class AuthenticateWithTokenMessage : RPCClientMessage
    {
        [ProtoMember(1)]
        public string Token { get; set; }
    }
}
