using ProtoBuf;

namespace NPSharp.Rpc.Packets
{
    [Packet(2002)]
    [ProtoContract]
    class MessagingSendDataMessage : RpcClientMessage
    {
        [ProtoMember(1)]
        public ulong NPID { get; set; }

        [ProtoMember(2)]
        public byte[] Data { get; set; }
    }
}