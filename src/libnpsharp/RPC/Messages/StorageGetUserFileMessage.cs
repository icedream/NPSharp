using ProtoBuf;

namespace NPSharp.RPC.Messages
{
    [Packet(1102)]
    [ProtoContract]
    public sealed class StorageGetUserFileMessage : RPCClientMessage
    {
        [ProtoMember(1)]
        public string FileName { get; set; }
        
        [ProtoMember(2)]
        public ulong NPID { get; set; } // SERIOUSLY WHY IS THIS EVEN HERE
    }
}