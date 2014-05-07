using ProtoBuf;

namespace NPSharp.Rpc.Packets
{
    [Packet(1104)]
    [ProtoContract]
    class StorageSendRandomStringMessage : RpcClientMessage
    {
        [ProtoMember(1)]
        public string RandomString { get; set; }
    }
}