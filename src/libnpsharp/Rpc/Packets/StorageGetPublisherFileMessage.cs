using ProtoBuf;

namespace NPSharp.RPC.Packets
{
    [ProtoContract]
    [Packet(1101)]
    class StorageGetPublisherFileMessage : RPCClientMessage
    {
        [ProtoMember(1)]
        public string FileName { get; set; }
    }
}
