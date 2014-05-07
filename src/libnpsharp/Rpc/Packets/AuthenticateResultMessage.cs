using ProtoBuf;

namespace NPSharp.Rpc.Packets
{
    [Packet(1010)]
    [ProtoContract]
    class AuthenticateResultMessage : RpcServerMessage
    {
        [ProtoMember(1)]
        public int Result { get; set; }

        [ProtoMember(2)]
        public ulong NPID { get; set; }

        [ProtoMember(3)]
        public string SessionToken { get; set; }
    }
}