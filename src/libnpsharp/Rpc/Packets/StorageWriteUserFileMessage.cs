using ProtoBuf;

namespace NPSharp.Rpc.Packets
{
    [Packet(1103)]
    [ProtoContract]
    class StorageWriteUserFileMessage : RpcClientMessage
    {
        [ProtoMember(1)]
        public string FileName { get; set; }

        [ProtoMember(2)]
        public ulong NPID { get; set; }

        [ProtoMember(3)]
        public byte[] FileData { get; set; }
    }
}