using ProtoBuf;

namespace NPSharp.RPC.Messages
{
    [Packet(1000)]
    [ProtoContract]
    public sealed class HelloMessage : RPCServerMessage
    {
        // I seriously have no idea where in the code this is used but whatever
        [ProtoMember(1)]
        public int Number1 { get; set; }

        [ProtoMember(2)]
        public int Number2 { get; set; }

        [ProtoMember(3)]
        public string Name { get; set; }

        [ProtoMember(4)]
        public string String2 { get; set; }
    }
}