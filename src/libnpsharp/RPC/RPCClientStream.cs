using System;
using System.Collections.Generic;
using System.Net.Sockets;
using log4net;
using NPSharp.RPC.Packets;

namespace NPSharp.RPC
{
    /// <summary>
    /// Represents a low-level client stream which can communicate with an NP server using RPC packets.
    /// </summary>
    public class RPCClientStream
    {
        private NetworkStream _ns;
        private uint _id;
        private ILog _log;

        private readonly string _host;
        private readonly ushort _port;

        private readonly Dictionary<uint, Action<RPCServerMessage>> _callbacks = new Dictionary<uint, Action<RPCServerMessage>>(); 

        /// <summary>
        /// Initializes an RPC connection stream with a specified host and port.
        /// </summary>
        /// <param name="host">The host to connect to.</param>
        /// <param name="port">The port to use. Default: 3025.</param>
        public RPCClientStream(string host, ushort port = 3025)
        {
            _host = host;
            _port = port;
            _log = LogManager.GetLogger("RPC");
        }

        /// <summary>
        /// Opens the RPC stream to the NP server.
        /// </summary>
        /// <returns>True if the connection succeeded, otherwise false.</returns>
        public bool Open()
        {
            // Connection already established?
            if (_ns != null)
                throw new InvalidOperationException("Connection already opened");

            var tcp = new TcpClient();
            try
            {
                tcp.Connect(_host, _port);
            }
            catch
            {
                return false;
            }
            _ns = tcp.GetStream();
            return true;
        }

        /// <summary>
        /// Closes the connection with the NP server.
        /// </summary>
        /// <param name="timeout"></param>
        public void Close(int timeout = 2000)
        {
            // Connection already closed?
            if (_ns == null)
                throw new InvalidOperationException("Connection already closed");

            try
            {
                _ns.Close(timeout);
                _ns.Dispose();
            }
            finally
            {
                _ns = null;
            }
        }

        /// <summary>
        /// Attaches a callback to the next message being sent out. This allows handling response packets.
        /// </summary>
        /// <param name="callback">The method to call when we receive a response to the next message</param>
        public void AttachCallback(Action<RPCServerMessage> callback)
        {
            if (_callbacks.ContainsKey(_id))
                throw new Exception("There is already a callback for the current message. You can only add max. one callback.");
            _callbacks.Add(_id, callback);
        }

        // TODO: Exposure of message ID needed or no?
        /// <summary>
        /// Sends out an RPC message.
        /// </summary>
        /// <param name="message">The RPC message to send out.</param>
        /// <returns>The new ID of the message.</returns>
        public uint Send(RPCClientMessage message)
        {
            if (_ns == null)
                throw new InvalidOperationException("You need to open the stream first.");

            var buffer = message.Serialize(_id);

            _ns.Write(buffer, 0, buffer.Length);
            _ns.Flush();

            _log.DebugFormat("Sent packet ID {1} (type {0})", message.GetType().Name, _id);

            return _id++;
        }

        /// <summary>
        /// Waits for the next RPC message from the server and reads it.
        /// </summary>
        /// <returns>The received server message.</returns>
        public RPCServerMessage Read()
        {
            if (_ns == null)
                throw new InvalidOperationException("You need to open the stream first.");

            var message = RPCServerMessage.Deserialize(_ns);

            if (message == null)
                return null;

            if (!_callbacks.ContainsKey(message.MessageId))
                return message;

            _callbacks[message.MessageId].Invoke(message);
            _callbacks.Remove(message.MessageId);

            _log.DebugFormat("Received packet ID {1} (type {0})", message.GetType().Name, message.MessageId);

            return message;
        }
    }
}
