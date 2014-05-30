using ProtoBuf;

namespace NPSharp.RPC.Messages.Server
{
    [Packet(1010)]
    [ProtoContract]
    public sealed class AuthenticateResultMessage : RPCServerMessage
    {
        [ProtoMember(1)]
        public int Result { get; set; }

        [ProtoMember(2)]
        public ulong NPID { get; set; }

        [ProtoMember(3)]
        public string SessionToken { get; set; }
    }
}