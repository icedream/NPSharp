using ProtoBuf;

namespace NPSharp.RPC.Packets
{
    [Packet(1104)]
    [ProtoContract]
    class StorageSendRandomStringMessage : RPCClientMessage
    {
        [ProtoMember(1)]
        public string RandomString { get; set; }
    }
}