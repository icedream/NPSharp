using ProtoBuf;

namespace NPSharp.RPC.Messages
{
    [Packet(1111)]
    [ProtoContract]
    public sealed class StoragePublisherFileMessage : RPCServerMessage
    {
        [ProtoMember(1)]
        public int Result { get; set; }

        [ProtoMember(2)]
        public string FileName { get; set; }

        [ProtoMember(3)]
        public byte[] FileData { get; set; }
    }
}