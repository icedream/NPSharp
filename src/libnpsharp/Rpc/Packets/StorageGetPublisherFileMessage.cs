using ProtoBuf;

namespace NPSharp.Rpc.Packets
{
    [ProtoContract]
    [Packet(1101)]
    class StorageGetPublisherFileMessage : RpcClientMessage
    {
        [ProtoMember(1)]
        public string FileName { get; set; }
    }
}
