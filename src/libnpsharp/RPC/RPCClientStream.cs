using System.Net.Sockets;
using NPSharp.RPC.Messages;

namespace NPSharp.RPC
{
    /// <summary>
    ///     Represents a low-level client stream which can communicate with an NP server using RPC messages.
    /// </summary>
    public class RPCClientStream : RPCStream<RPCClientMessage, RPCServerMessage>
    {
        public RPCClientStream(Socket sock) : base(sock)
        {
        }

        public static RPCClientStream Open(string host, ushort port = 3025)
        {
            var sock = new Socket(
                AddressFamily.InterNetwork,
                SocketType.Stream,
                ProtocolType.Tcp);
            sock.Connect(host, port);
            return new RPCClientStream(sock);
        }
    }
}