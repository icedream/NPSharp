using ProtoBuf;

namespace NPSharp.RPC.Packets
{
    [Packet(2002)]
    [ProtoContract]
    class MessagingSendDataMessage : RPCClientMessage
    {
        [ProtoMember(1)]
        public ulong NPID { get; set; }

        [ProtoMember(2)]
        public byte[] Data { get; set; }
    }
}