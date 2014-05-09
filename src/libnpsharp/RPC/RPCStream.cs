using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using log4net;
using NPSharp.RPC.Messages;

namespace NPSharp.RPC
{
    /// <summary>
    ///     Represents a low-level client stream which can communicate using RPC packets.
    /// </summary>
    public abstract class RPCStream<TSend, TRecv>
        where TSend : RPCMessage
        where TRecv : RPCMessage
    {
        /// <summary>
        ///     Registered callbacks for all received messages.
        /// </summary>
        protected readonly List<Action<TRecv>> GeneralCallbacks =
            new List<Action<TRecv>>();

        /// <summary>
        ///     Registered callbacks for specific received message IDs.
        /// </summary>
        protected readonly List<KeyValuePair<uint, Action<TRecv>>> IDCallbacks =
            new List<KeyValuePair<uint, Action<TRecv>>>();

        /// <summary>
        ///     Registered callbacks for specific received message type IDs.
        /// </summary>
        protected readonly List<KeyValuePair<uint, Action<TRecv>>> TypeCallbacks =
            new List<KeyValuePair<uint, Action<TRecv>>>();

        /// <summary>
        ///     Logger instance.
        /// </summary>
        private readonly ILog _log;

        /// <summary>
        ///     ID of the next message.
        /// </summary>
        protected uint MessageID;

        /// <summary>
        ///     Base stream.
        /// </summary>
        private Socket _sock;

        /// <summary>
        ///     Initializes an RPC connection stream from an already established network connection.
        /// </summary>
        /// <param name="sock">Client's network stream</param>
        protected RPCStream(Socket sock)
        {
            _log = LogManager.GetLogger("RPC");
            _sock = sock;
        }

        /// <summary>
        ///     Sets the next message's ID.
        /// </summary>
        protected void IterateMessageID()
        {
            MessageID++;
        }

        /// <summary>
        ///     Closes the connection.
        /// </summary>
        /// <param name="timeout"></param>
        public void Close(int timeout = 2000)
        {
            // Connection already closed?
            if (_sock == null)
                throw new InvalidOperationException("Connection already closed");

            try
            {
                GeneralCallbacks.Clear();
                IDCallbacks.Clear();
                TypeCallbacks.Clear();

                _sock.Close(timeout);
                _sock.Dispose();
            }
            finally
            {
                _sock = null;
            }
        }

        /// <summary>
        ///     Attaches a callback to the connection which handles a specific incoming RPC message type.
        /// </summary>
        /// <typeparam name="T">The message type to handle, must be a subtype of RPCMessage</typeparam>
        /// <param name="callback"></param>
        public void AttachHandlerForMessageType<T>(Action<T> callback) where T : TRecv
        {
            TypeCallbacks.Add(
                new KeyValuePair<uint, Action<TRecv>>(
                    ((PacketAttribute) typeof (T).GetCustomAttributes(typeof (PacketAttribute), false).Single()).Type,
                    (Action<TRecv>)callback));
        }

        /// <summary>
        ///     Attaches a callback to the connection which handles a response to the next message we send.
        /// </summary>
        /// <param name="callback"></param>
        public void AttachHandlerForNextMessage(Action<TRecv> callback)
        {
            IDCallbacks.Add(new KeyValuePair<uint, Action<TRecv>>(MessageID, callback));
        }

        /// <summary>
        ///     Attaches a callback to the connection for all incoming RPC messages.
        /// </summary>
        /// <param name="callback"></param>
        public void AttachHandler(Action<TRecv> callback)
        {
            GeneralCallbacks.Add(callback);
        }

        /// <summary>
        ///     Sends out an RPC message to the remote endpoint.
        /// </summary>
        /// <param name="message">The RPC message to send out.</param>
        /// <returns>The new ID of the message.</returns>
        public void Send(TSend message)
        {
            if (_sock == null)
                throw new InvalidOperationException("You need to open the stream first.");

            message.MessageId = MessageID;

            var buffer = message.Serialize();

            _sock.Send(buffer);

            IterateMessageID();
        }

        /// <summary>
        ///     Waits for the next RPC message from the remote end and reads it.
        /// </summary>
        /// <returns>The received server message.</returns>
        public TRecv Read()
        {
            if (_sock == null)
                throw new InvalidOperationException("You need to open the stream first.");

            var message = RPCMessage.Deserialize<TRecv>(_sock);

            if (message == null)
            {
                return null;
            }

            // Callbacks
            foreach (var cbi in IDCallbacks.Where(p => p.Key == message.MessageId))
                cbi.Value.Invoke(message);
            foreach (var cbi in TypeCallbacks.Where(p => p.Key == message.GetTypeId()))
                cbi.Value.Invoke(message);
            foreach (var callback in GeneralCallbacks)
                callback.Invoke(message);

            return message;
        }
    }
}