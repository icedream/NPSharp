using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace NPSharp.Rpc.Packets
{
    public class RpcClientMessage : RpcMessage
    {
        public byte[] Serialize(uint id)
        {
            byte[] content;
            using (var bufferStream = new MemoryStream())
            {
                ProtoBuf.Serializer.Serialize(bufferStream, this);
                bufferStream.Seek(0, SeekOrigin.Begin);
                content = bufferStream.ToArray();
            }

            var buffer = new List<byte>();
            buffer.AddRange(BitConverter.GetBytes((uint)IPAddress.HostToNetworkOrder(Signature)));
            buffer.AddRange(BitConverter.GetBytes((uint)IPAddress.HostToNetworkOrder(content.Length)));
            buffer.AddRange(BitConverter.GetBytes((uint)IPAddress.HostToNetworkOrder(GetTypeId())));
            buffer.AddRange(BitConverter.GetBytes((uint)IPAddress.HostToNetworkOrder(id)));
            buffer.AddRange(content);

            return buffer.ToArray();
        }
    }
}