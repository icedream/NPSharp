using ProtoBuf;

namespace NPSharp.RPC.Packets
{
    [Packet(2001)]
    [ProtoContract]
    class CloseAppMessage : RPCServerMessage
    {
        [ProtoMember(1)]
        public string Reason { get; set; }
    }
}