using ProtoBuf;

namespace NPSharp.Rpc.Packets
{
    [Packet(1112)]
    [ProtoContract]
    class StorageUserFileMessage : RpcServerMessage
    {
        [ProtoMember(1)]
        public int Result { get; set; }

        [ProtoMember(2)]
        public string FileName { get; set; }

        [ProtoMember(3)]
        public ulong NPID { get; set; }

        [ProtoMember(4)]
        public byte[] FileData { get; set; }
    }
}