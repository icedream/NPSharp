using ProtoBuf;

namespace NPSharp.RPC.Packets
{
    [Packet(1102)]
    [ProtoContract]
    class StorageGetUserFileMessage : RPCClientMessage
    {
        [ProtoMember(1)]
        public string FileName { get; set; }
        
        [ProtoMember(2)]
        public ulong NPID { get; set; } // SERIOUSLY WHY IS THIS EVEN HERE
    }
}