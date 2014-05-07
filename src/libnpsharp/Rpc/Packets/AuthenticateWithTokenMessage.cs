using ProtoBuf;

namespace NPSharp.Rpc.Packets
{
    [Packet(1003)]
    [ProtoContract]
    class AuthenticateWithTokenMessage : RpcClientMessage
    {
        [ProtoMember(1)]
        public string Token { get; set; }
    }
}
