using System;
using ProtoBuf;

namespace NPSharp.RPC.Messages.Client
{
    [Packet(1004)]
    [ProtoContract]
    public sealed class AuthenticateValidateTicketMessage : RPCClientMessage
    {
        [ProtoMember(1, DataFormat = DataFormat.FixedSize)]
        public UInt32 ClientIP { get; set; }

        [ProtoMember(2, DataFormat = DataFormat.FixedSize)]
        public UInt64 NPID { get; set; }

        [ProtoMember(3)]
        public byte[] Ticket { get; set; }
    }
}