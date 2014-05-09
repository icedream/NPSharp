using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using NPSharp.RPC;
using NPSharp.RPC.Messages;
using NPSharp.Steam;

namespace NPSharp
{
    public class NPServer
    {
        private readonly List<NPServerClient> _clients;
        private readonly ILog _log;
        private readonly Socket _socket;
        private readonly ushort _port;

        /// <summary>
        ///     Constructs a new NP server.
        /// </summary>
        public NPServer(ushort port = 3025)
        {
            _log = LogManager.GetLogger("NPServer");
            _clients = new List<NPServerClient>();

            _socket = new Socket(SocketType.Stream, ProtocolType.IP);
            _port = port;
        }

        /// <summary>
        ///     Starts up the NP server.
        /// </summary>
        public void Start()
        {
            if (_socket.IsBound)
                throw new InvalidOperationException("This server is already running");

            try
            {
                // ReSharper disable once ObjectCreationAsStatement
                // TODO: fix this shit permission code
                new SocketPermission(NetworkAccess.Accept, TransportType.Tcp, "", _port);
            }
            catch
            {
                _log.Error("Socket permission request failed, can't start server.");
                throw new SocketException(10013 /* Permission denied */);
            }

            _socket.Bind(new IPEndPoint(IPAddress.IPv6Any, _port));
            _socket.Listen(4);

            Task.Factory.StartNew(() =>
            {
                _log.Debug("Listener loop started");
                var allDone = new ManualResetEvent(false);
                while (_socket != null && _socket.IsBound)
                {
                    allDone.Reset();
                    _socket.BeginAccept(ar =>
                    {
                        _log.Debug("Async accept client start");
                        allDone.Set();

                        var serverSocket = (Socket) ar.AsyncState;
                        var clientSocket = serverSocket.EndAccept(ar);

                        var npsc = new NPServerClient(this, new RPCServerStream(clientSocket));

                        _log.Debug("Async accept client end"); 
                        
                        _handleClient(npsc);
                    }, _socket);
                    allDone.WaitOne();
                }
                _log.Debug("Listener loop shut down");
            });
        }

        /// <summary>
        ///     Shuts down all connections and stops the NP server.
        /// </summary>
        public void Stop()
        {
            _socket.Shutdown(SocketShutdown.Both);
        }

        /// <summary>
        ///     The handler to use for file requests to this NP server.
        /// </summary>
        public IFileServingHandler FileHandler { get; set; }

        /// <summary>
        ///     The handler to use for user avatar requests to this NP server.
        /// </summary>
        public IUserAvatarHandler UserAvatarHandler { get; set; }

        /// <summary>
        ///     Returns all currently connected clients
        /// </summary>
        public NPServerClient[] Clients
        {
            get { return _clients.ToArray(); } // Avoid race condition by IEnum changes
        }

        /// <summary>
        ///     The handler to use for authentication requests to this NP server.
        /// </summary>
        public IAuthenticationHandler AuthenticationHandler { get; set; }

        /// <summary>
        ///     The handler to use for friends-related requests to this NP server.
        /// </summary>
        public IFriendsHandler FriendsHandler { get; set; }

        internal void _handleClient(NPServerClient client)
        {
            _log.Debug("Client now being handled"); 

            #region RPC authentication message handlers

            client.RPC.AttachHandlerForMessageType<AuthenticateWithKeyMessage>(msg =>
            {
                var result = new AuthenticationResult();
                if (AuthenticationHandler != null)
                {
                    try
                    {
                        result = AuthenticationHandler.AuthenticateServer(client, msg.LicenseKey);
                    }
                    catch (Exception error)
                    {
                        _log.Error("Error occurred in authentication handler", error);
                    }
                }

                // Send authentication result directly to client
                client.RPC.Send(new AuthenticateResultMessage
                {
                    NPID = result.UserID,
                    Result = result.Result ? 0 : 1,
                    SessionToken = string.Empty
                });

                // Authentication failed => close connection
                if (!result.Result)
                {
                    client.RPC.Close();
                    return;
                }

                // Assign login ID
                client.UserID = result.UserID;
                client.IsServer = true;

                OnClientAuthenticated(client);
            });

            client.RPC.AttachHandlerForMessageType<AuthenticateWithDetailsMessage>(msg =>
            {
                var result = new AuthenticationResult();
                if (AuthenticationHandler != null)
                {
                    try
                    {
                        result = AuthenticationHandler.AuthenticateUser(client, msg.Username, msg.Password);
                    }
                    catch (Exception error)
                    {
                        _log.Error("Error occurred in authentication handler", error);
                        result = new AuthenticationResult();
                    }
                }

                // Send authentication result directly to client
                client.RPC.Send(new AuthenticateResultMessage
                {
                    NPID = result.UserID,
                    Result = result.Result ? 0 : 1,
                    SessionToken = string.Empty
                });

                // Authentication failed => close connection
                if (!result.Result)
                {
                    client.RPC.Close();
                    return;
                }

                // Assign login ID
                client.UserID = result.UserID;

                // Send "online" notification to all friends of this player
                foreach (NPServerClient fconn in client.FriendConnections)
                {
                    fconn.RPC.Send(new FriendsPresenceMessage
                    {
                        CurrentServer = client.DedicatedServer == null ? 0 : client.DedicatedServer.UserID,
                        Friend = client.UserID,
                        Presence = client.PresenceData,
                        PresenceState = client.DedicatedServer == null ? 1 : 2
                    });
                }

                OnClientAuthenticated(client);
            });

            client.RPC.AttachHandlerForMessageType<AuthenticateWithTokenMessage>(msg =>
            {
                var result = new AuthenticationResult();
                if (AuthenticationHandler != null)
                {
                    try
                    {
                        result = AuthenticationHandler.AuthenticateUser(client, msg.Token);
                    }
                    catch (Exception error)
                    {
                        _log.Error("Error occurred in authentication handler", error);
                    }
                }

                // Send authentication result directly to client
                client.RPC.Send(new AuthenticateResultMessage
                {
                    NPID = result.UserID,
                    Result = result.Result ? 0 : 1,
                    SessionToken = msg.Token
                });

                // Authentication failed => close connection
                if (!result.Result)
                {
                    client.RPC.Close();
                    return;
                }

                // Assign login ID
                client.UserID = result.UserID;

                // Send "online" notification to all friends of this player
                foreach (NPServerClient fconn in client.FriendConnections)
                {
                    fconn.RPC.Send(new FriendsPresenceMessage
                    {
                        CurrentServer = client.DedicatedServer == null ? 0 : client.DedicatedServer.UserID,
                        Friend = client.UserID,
                        Presence = client.PresenceData,
                        PresenceState = client.DedicatedServer == null ? 1 : 2
                    });
                }

                OnClientAuthenticated(client);
            });

            client.RPC.AttachHandlerForMessageType<AuthenticateValidateTicketMessage>(msg =>
            {
                var validTicket = false;

                if (!client.IsDirty)
                {
                    Ticket ticketData = null;

                    try
                    {
                        ticketData = new Ticket(msg.Ticket);

                        _log.DebugFormat("Ticket[Version={0},ServerID={1},Time={2}]", ticketData.Version,
                            ticketData.ServerID, ticketData.Time);
                    }
                    catch (ArgumentException error)
                    {
                        _log.Warn("Got some weird-length ticket data", error);
                    }

                    if (ticketData != null)
                    {
                        if (ticketData.Version == 1) // Version 1 enforcement
                        {
                            if (ticketData.ClientID == client.UserID) // NPID enforcement
                            {
                                var s =
                                    _clients.Where(c => c.IsServer && !c.IsDirty && c.UserID == ticketData.ServerID)
                                        .ToArray();

                                if (s.Any())
                                {
                                    // TODO: Time validation. Problem is some clocks go wrong by minutes!
                                    client.DedicatedServer = s.First();
                                    validTicket = true;
                                    _log.Debug("Ticket validated");
                                }
                                else
                                {
                                    _log.Warn("Ticket invalid, could not find any sane servers with requested server ID");
                                }
                            }
                            else
                            {
                                _log.Warn("Ticket invalid, found NPID spoofing attempt");
                            }
                        }
                        else
                        {
                            _log.Warn("Ticket invalid, found invalid version");
                        }
                    }
                }
                else
                {
                    _log.Warn("Ticket invalid, client is marked as dirty");
                }

                // Invalid data buffer
                client.RPC.Send(new AuthenticateValidateTicketResultMessage
                {
                    GroupID = client.GroupID,
                    NPID = client.UserID,
                    Result = validTicket ? 0 : 1
                });
            });

            #endregion

            #region RPC friend message handlers

            client.RPC.AttachHandlerForMessageType<FriendsSetPresenceMessage>(msg =>
            {
                foreach (FriendsPresence pdata in msg.Presence)
                {
                    client.SetPresence(pdata.Key, pdata.Value);
                    _log.DebugFormat("Client says presence \"{0}\" is \"{1}\"", pdata.Key, pdata.Value);
                }
            });

            client.RPC.AttachHandlerForMessageType<FriendsGetUserAvatarMessage>(msg =>
            {
                // Why so goddamn complicated, NTA. Fuck.
                // TODO: Not compatible with non-public accounts
                ulong npid = new CSteamID((uint) msg.Guid, EUniverse.Public,
                    EAccountType.Individual).ConvertToUint64();

                byte[] avatar = UserAvatarHandler.GetUserAvatar(npid) ?? UserAvatarHandler.GetDefaultAvatar();

                client.RPC.Send(new FriendsGetUserAvatarResultMessage
                {
                    FileData = avatar,
                    Guid = msg.Guid,
                    Result = avatar != null ? 0 : 1
                });
            });

            client.RPC.AttachHandlerForMessageType<FriendsGetProfileDataMessage>(msg =>
            {
                // TODO
            });

            #endregion

            #region RPC storage message handlers

            client.RPC.AttachHandlerForMessageType<StorageGetPublisherFileMessage>(msg =>
            {
                if (FileHandler == null)
                {
                    client.RPC.Send(new StoragePublisherFileMessage
                    {
                        MessageId = msg.MessageId,
                        Result = 2,
                        FileName = msg.FileName
                    });
                    return;
                }

                try
                {
                    if (client.UserID == null)
                    {
                        client.RPC.Send(new StoragePublisherFileMessage
                        {
                            MessageId = msg.MessageId,
                            Result = 2,
                            FileName = msg.FileName
                        });
                        _log.WarnFormat("Client tried to read publisher file {0} while not logged in", msg.FileName);
                        return;
                    }

                    byte[] data = FileHandler.ReadPublisherFile(client, msg.FileName);
                    if (data == null)
                    {
                        client.RPC.Send(new StoragePublisherFileMessage
                        {
                            MessageId = msg.MessageId,
                            Result = 1,
                            FileName = msg.FileName
                        });
                        _log.DebugFormat("Could not open publisher file {0}", msg.FileName);
                        return;
                    }

                    client.RPC.Send(new StoragePublisherFileMessage
                    {
                        MessageId = msg.MessageId,
                        Result = 0,
                        FileName = msg.FileName
                    });
                    _log.DebugFormat("Sent publisher file {0}", msg.FileName);
                }
                catch (Exception error)
                {
                    _log.Warn("GetPublisherFile handler error", error);
                    client.RPC.Send(new StoragePublisherFileMessage
                    {
                        MessageId = msg.MessageId,
                        Result = 2,
                        FileName = msg.FileName
                    });
                }
            });

            client.RPC.AttachHandlerForMessageType<StorageGetUserFileMessage>(msg =>
            {
                if (FileHandler == null)
                {
                    client.RPC.Send(new StorageUserFileMessage
                    {
                        MessageId = msg.MessageId,
                        Result = 2,
                        FileName = msg.FileName
                    });
                    return;
                }

                try
                {
                    if (client.UserID == null)
                    {
                        client.RPC.Send(new StorageUserFileMessage
                        {
                            MessageId = msg.MessageId,
                            Result = 2,
                            FileName = msg.FileName,
                            NPID = client.UserID
                        });
                        _log.WarnFormat("Client tried to read user file {0} while not logged in", msg.FileName);
                        return;
                    }

                    byte[] data = FileHandler.ReadUserFile(client, msg.FileName);
                    if (data == null)
                    {
                        client.RPC.Send(new StorageUserFileMessage
                        {
                            MessageId = msg.MessageId,
                            Result = 1,
                            FileName = msg.FileName,
                            NPID = client.UserID
                        });
                        _log.DebugFormat("Could not open user file {0}", msg.FileName);
                        return;
                    }

                    client.RPC.Send(new StorageUserFileMessage
                    {
                        MessageId = msg.MessageId,
                        Result = 0,
                        FileName = msg.FileName,
                        FileData = data,
                        NPID = client.UserID
                    });
                    _log.DebugFormat("Sent user file {0}", msg.FileName);
                }
                catch (Exception error)
                {
                    _log.Warn("GetUserFile handler error", error);
                    client.RPC.Send(new StorageUserFileMessage
                    {
                        MessageId = msg.MessageId,
                        Result = 2,
                        FileName = msg.FileName
                    });
                }
            });

            client.RPC.AttachHandlerForMessageType<StorageSendRandomStringMessage>(msg =>
            {
                // TODO: Handle "random string" messages
            });

            client.RPC.AttachHandlerForMessageType<StorageWriteUserFileMessage>(msg =>
            {
                if (FileHandler == null)
                {
                    client.RPC.Send(new StorageWriteUserFileResultMessage
                    {
                        MessageId = msg.MessageId,
                        Result = 2,
                        FileName = msg.FileName
                    });
                    return;
                }

                try
                {
                    if (client.UserID == null)
                    {
                        client.RPC.Send(new StorageWriteUserFileResultMessage
                        {
                            MessageId = msg.MessageId,
                            Result = 2,
                            FileName = msg.FileName,
                            NPID = client.UserID
                        });
                        _log.WarnFormat("Client tried to write user file {0} while not logged in", msg.FileName);
                        return;
                    }

                    FileHandler.WriteUserFile(client, msg.FileName, msg.FileData);

                    client.RPC.Send(new StorageWriteUserFileResultMessage
                    {
                        MessageId = msg.MessageId,
                        Result = 0,
                        FileName = msg.FileName,
                        NPID = client.UserID
                    });
                    _log.DebugFormat("Received and wrote user file {0}", msg.FileName);
                }
                catch (Exception error)
                {
                    _log.Warn("WriteUserFile handler error", error);
                    client.RPC.Send(new StorageWriteUserFileResultMessage
                    {
                        MessageId = msg.MessageId,
                        Result = 2,
                        FileName = msg.FileName
                    });
                }
            });
            // TODO: RPC message handling for MessagingSendData

            #endregion

            #region RPC server session message handler

            #endregion

            _clients.Add(client);
            try
            {
                _log.Debug("Client connected");
                OnClientConnected(client);
                while (true)
                {
                    var msg = client.RPC.Read();
                    if (msg == null)
                        break;
                }
                _log.Debug("Client disconnected");
                OnClientDisconnected(client);
            }
            catch (Exception error)
            {
                _log.Error("Error in client handling loop", error);
                client.RPC.Send(new CloseAppMessage {Reason = "Server-side error occurred, try again later."});
                client.RPC.Close();
            }
            _clients.Remove(client);
        }

        /// <summary>
        ///     Triggered when a client has connected but is not authenticating yet.
        /// </summary>
        public event ClientEventHandler ClientConnected;

        /// <summary>
        ///     Invokes the <see cref="ClientConnected" /> event.
        /// </summary>
        /// <param name="client">The client</param>
        protected virtual void OnClientConnected(NPServerClient client)
        {
            var handler = ClientConnected;
            var args = new ClientEventArgs(client);
            if (handler != null) handler(this, args);
        }

        /// <summary>
        ///     Triggered when a client has disconnected.
        /// </summary>
        public event ClientEventHandler ClientDisconnected;

        /// <summary>
        ///     Invokes the <see cref="ClientDisconnected" /> event.
        /// </summary>
        /// <param name="client">The client</param>
        protected virtual void OnClientDisconnected(NPServerClient client)
        {
            var handler = ClientDisconnected;
            var args = new ClientEventArgs(client);
            if (handler != null) handler(this, args);
        }

        /// <summary>
        ///     Triggered when a client has authenticated successfully.
        /// </summary>
        public event ClientEventHandler ClientAuthenticated;

        /// <summary>
        ///     Invokes the <see cref="ClientAuthenticated" /> event.
        /// </summary>
        /// <param name="client">The client</param>
        protected virtual void OnClientAuthenticated(NPServerClient client)
        {
            var handler = ClientAuthenticated;
            var args = new ClientEventArgs(client);
            if (handler != null) handler(this, args);
        }
    }
}