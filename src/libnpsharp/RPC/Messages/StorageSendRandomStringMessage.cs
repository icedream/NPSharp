using ProtoBuf;

namespace NPSharp.RPC.Messages
{
    [Packet(1104)]
    [ProtoContract]
    public sealed class StorageSendRandomStringMessage : RPCClientMessage
    {
        [ProtoMember(1)]
        public string RandomString { get; set; }
    }
}