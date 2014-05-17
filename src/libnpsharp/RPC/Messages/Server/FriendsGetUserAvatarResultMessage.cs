using ProtoBuf;

namespace NPSharp.RPC.Messages.Server
{
    [ProtoContract]
    [Packet(1215)]
    public sealed class FriendsGetUserAvatarResultMessage : RPCServerMessage
    {
        internal FriendsGetUserAvatarResultMessage()
        {
        }

        [ProtoMember(1)]
        public int Result { get; internal set; }

        [ProtoMember(2)]
        public int Guid { get; internal set; }

        [ProtoMember(3)]
        public byte[] FileData { get; internal set; }
    }
}