using ProtoBuf;

namespace NPSharp.RPC.Messages.Client
{
    [Packet(1103)]
    [ProtoContract]
    public sealed class StorageWriteUserFileMessage : RPCClientMessage
    {
        [ProtoMember(1)]
        public string FileName { get; set; }

        [ProtoMember(2)]
        public ulong NPID { get; set; }

        [ProtoMember(3)]
        public byte[] FileData { get; set; }
    }
}