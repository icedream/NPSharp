using System;
using ProtoBuf;

namespace NPSharp.RPC.Messages
{
    [Packet(1012)]
    [ProtoContract]
    public sealed class AuthenticateValidateTicketResultMessage : RPCServerMessage
    {
        [ProtoMember(1)]
        public int Result { get; set; }

        [ProtoMember(2, DataFormat = DataFormat.FixedSize)]
        public UInt64 NPID { get; set; }

        [ProtoMember(3)]
        public int GroupID { get; set; }
    }
}