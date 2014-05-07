using ProtoBuf;

namespace NPSharp.Rpc.Packets
{
    [Packet(1102)]
    [ProtoContract]
    class StorageGetUserFileMessage : RpcClientMessage
    {
        [ProtoMember(1)]
        public string FileName { get; set; }
        
        [ProtoMember(2)]
        public ulong NPID { get; set; } // SERIOUSLY WHY IS THIS EVEN HERE
    }
}