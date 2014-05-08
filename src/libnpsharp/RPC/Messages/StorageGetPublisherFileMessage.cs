using ProtoBuf;

namespace NPSharp.RPC.Messages
{
    [ProtoContract]
    [Packet(1101)]
    public sealed class StorageGetPublisherFileMessage : RPCClientMessage
    {
        [ProtoMember(1)]
        public string FileName { get; set; }
    }
}
