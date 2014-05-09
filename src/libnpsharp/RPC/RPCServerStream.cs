using System.Net.Sockets;
using NPSharp.RPC.Messages;

namespace NPSharp.RPC
{
    /// <summary>
    ///     Represents a low-level stream which communicates with an NP client using RPC messages.
    /// </summary>
    public class RPCServerStream : RPCStream<RPCServerMessage, RPCClientMessage>
    {
        public RPCServerStream(Socket sock) : base(sock)
        {
        }
    }
}