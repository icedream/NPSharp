using ProtoBuf;

namespace NPSharp.Rpc.Packets
{
    [Packet(1113)]
    [ProtoContract]
    class StorageWriteUserFileResultMessage : RpcServerMessage
    {
        [ProtoMember(1)]
        public int Result { get; set; }

        [ProtoMember(2)]
        public string FileName { get; set; }
 
        [ProtoMember(3)]
        public ulong NPID { get; set; }
    }
}