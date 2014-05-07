using ProtoBuf;

namespace NPSharp.RPC.Packets
{
    [Packet(1111)]
    [ProtoContract]
    class StoragePublisherFileMessage : RPCServerMessage
    {
        [ProtoMember(1)]
        public int Result { get; set; }

        [ProtoMember(2)]
        public string FileName { get; set; }

        [ProtoMember(3)]
        public byte[] FileData { get; set; }
    }
}