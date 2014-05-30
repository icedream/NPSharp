using ProtoBuf;

namespace NPSharp.RPC.Messages.Server
{
    [Packet(1112)]
    [ProtoContract]
    public sealed class StorageUserFileMessage : RPCServerMessage
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