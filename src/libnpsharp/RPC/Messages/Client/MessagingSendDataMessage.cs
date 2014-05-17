using ProtoBuf;

namespace NPSharp.RPC.Messages.Client
{
    [Packet(2002)]
    [ProtoContract]
    public sealed class MessagingSendDataMessage : RPCClientMessage
    {
        [ProtoMember(1)]
        public ulong NPID { get; set; }

        [ProtoMember(2)]
        public byte[] Data { get; set; }
    }
}