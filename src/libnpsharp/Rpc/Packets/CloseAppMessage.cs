using ProtoBuf;

namespace NPSharp.Rpc.Packets
{
    [Packet(2001)]
    [ProtoContract]
    class CloseAppMessage : RpcServerMessage
    {
        [ProtoMember(1)]
        public string Reason { get; set; }
    }
}