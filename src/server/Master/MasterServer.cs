using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using NPSharp.Master.Messages;
using NPSharp.Master.Messages.Client;
using NPSharp.NP;

namespace NPSharp.Master
{
    public class MasterServer
    {
        // TODO: !! Avoid socket fail if stopping then restarting
        private readonly Socket _socket4 = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        private readonly Socket _socket6 = new Socket(AddressFamily.InterNetworkV6, SocketType.Dgram, ProtocolType.Udp);

        private readonly ILog _log;
        private readonly ushort _port;

        private readonly List<KeyValuePair<Type, Action<MasterClientMessage>>> _callbacks =
            new List<KeyValuePair<Type, Action<MasterClientMessage>>>();

        // TODO: Use the same kind of interfaces as in NP server to handle server addition and deletion
        private readonly List<DedicatedServerEntry> _registeredServers = new List<DedicatedServerEntry>();

        public MasterServer(ushort port = 20810)
        {
            _port = port;
            _log = LogManager.GetLogger("MasterServer");

            // Internal callbacks
            AddCallback<MasterGetServersMessage>(messages =>
            {
                
            });
        }

        internal void AddCallback<T>(Action<T> callback) where T : MasterClientMessage
        {
            _callbacks.Add(
                new KeyValuePair<Type, Action<MasterClientMessage>>(
                    typeof(T),
                    msg => callback.Invoke((T)msg)));
        }

        /// <summary>
        ///     Starts up the NP server.
        /// </summary>
        public void Start()
        {
            if (_socket4.IsBound || _socket6.IsBound)
                throw new InvalidOperationException("This server is already running");

            try
            {
                // ReSharper disable once ObjectCreationAsStatement
                // TODO: fix this shit permission code
                new SocketPermission(NetworkAccess.Accept, TransportType.Udp, "", _port);
            }
            catch
            {
                _log.Error("Socket permission request failed, can't start server.");
                throw new SocketException(10013 /* Permission denied */);
            }

            _socket4.Bind(new IPEndPoint(IPAddress.Any, _port));
            _socket4.Listen(100);

            _socket6.Bind(new IPEndPoint(IPAddress.IPv6Any, _port));
            _socket6.Listen(100);


            // TODO: Implement IPv4 handling

            Task.Factory.StartNew(() =>
            {
                _log.Debug("Listener loop (IPv6) started");

                while (_socket6 != null && _socket6.IsBound)
                {
                    var mergedBuffer = new List<byte>();
                    while (true)
                    {
                        var buffer = new byte[1400];
                        var clientEndPoint = (EndPoint)new IPEndPoint(IPAddress.IPv6Any, 0);
                        var recvLength = _socket6.ReceiveFrom(buffer, ref clientEndPoint);
                        if (recvLength <= buffer.Length)
                            mergedBuffer.AddRange(buffer);
                        if (recvLength < 1400)
                            break;
                        _handleClient(buffer, clientEndPoint);
                    }
                }
                _log.Debug("Listener loop (IPv6) shut down");
            });
        }

        private void _handleClient(byte[] buffer, EndPoint ep)
        {
            _log.DebugFormat("Handle client {0}", ep);

            var message = MasterClientMessage.Deserialize(buffer);
            if (message == null)
            {
                _log.WarnFormat("Received invalid or empty request from {0}", ep);
                return;
            }

            // Invoke (internal) callbacks for fitting message types
            foreach (var callback in _callbacks.Where(i => i.Key == message.GetType()).Select(i => i.Value))
            {
                callback.Invoke(message);
            }

            _log.DebugFormat("Not handling client {0} anymore", ep);
        }

    }
}
