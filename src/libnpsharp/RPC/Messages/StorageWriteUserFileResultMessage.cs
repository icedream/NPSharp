using ProtoBuf;

namespace NPSharp.RPC.Messages
{
    [Packet(1113)]
    [ProtoContract]
    public sealed class StorageWriteUserFileResultMessage : RPCServerMessage
    {
        [ProtoMember(1)]
        public int Result { get; set; }

        [ProtoMember(2)]
        public string FileName { get; set; }
 
        [ProtoMember(3)]
        public ulong NPID { get; set; }
    }
}