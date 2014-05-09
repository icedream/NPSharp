using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Configuration;
using log4net;
using NPSharp.RPC;
using NPSharp.RPC.Messages;
using NPSharp.Steam;

namespace NPSharp
{
    public class NPServer
    {
        public delegate void ClientEventHandler(object sender, ClientEventArgs args);

        public IFileServingHandler FileHandler { get; set; }

        public IUserAvatarHandler UserAvatarHandler { get; set; }

        public interface IUserAvatarHandler
        {
            byte[] GetUserAvatar(CSteamID id);

            byte[] GetDefaultAvatar();
        }

        private readonly List<NPServerClient> _clients;
        private readonly ILog _log;

        public NPServer()
        {
            _log = LogManager.GetLogger("NPServer");
            _clients = new List<NPServerClient>();
        }

        private void _handleClient(NPServerClient client)
        {
            #region RPC authentication message handlers
            client.RPC.AttachHandlerForMessageType<AuthenticateWithKeyMessage>(msg =>
            {
                var result = new AuthenticationResult();;
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
                foreach (var fconn in client.FriendConnections)
                {
                    fconn.RPC.Send(new FriendsPresenceMessage
                    {
                        CurrentServer = client.DedicatedServer == null ? 0 : client.DedicatedServer.NPID,
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
                foreach (var fconn in client.FriendConnections)
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
                                var s = _clients.Where(c => c.IsServer && !c.IsDirty && c.UserID == ticketData.ServerID).ToArray();

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
                foreach (var pdata in msg.Presence)
                {
                    client.SetPresence(pdata.Key, pdata.Value);
                    _log.DebugFormat("Client says presence \"{0}\" is \"{1}\"", pdata.Key, pdata.Value);
                }
            });

            client.RPC.AttachHandlerForMessageType<FriendsGetUserAvatarMessage>(msg =>
            {
                // Why so goddamn complicated, NTA. Fuck.
                // TODO: Not compatible with non-public accounts
                var npid = new CSteamID((uint) msg.Guid, EUniverse.Public,
                    EAccountType.Individual).ConvertToUint64();

                var avatar = UserAvatarHandler.GetUserAvatar(npid) ?? UserAvatarHandler.GetDefaultAvatar();

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

            // TODO: RPC message handling for storage
            // TODO: RPC message handling for MessagingSendData
            // TODO: RPC message handling for server sessions

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
                client.RPC.Send(new CloseAppMessage{Reason="Server-side error occurred, try again later."});
                client.RPC.Close();
            }
            _clients.Remove(client);
        }

        public NPServerClient[] Clients
        {
            get { return _clients.ToArray(); } // Avoid race condition by IEnum changes
        }

        public interface IFileServingHandler
        {
            Stream ReadUserFile(NPServerClient client, string file);

            Stream ReadPublisherFile(NPServerClient client, string file);
        }

        public IAuthenticationHandler AuthenticationHandler { get; set; }

        public interface IAuthenticationHandler
        {
            AuthenticationResult AuthenticateUser(NPServerClient client, string username, string password);

            AuthenticationResult AuthenticateUser(NPServerClient client, string token);

            AuthenticationResult AuthenticateServer(NPServerClient client, string licenseKey);

            TicketValidationResult ValidateTicket(NPServerClient client, NPServerClient server);
        }

        public class AuthenticationResult
        {
            /// <summary>
            /// Constructs an authentication result instance.
            /// </summary>
            /// <param name="npid">Set this to null if authentication should fail, otherwise use an instance of a steam ID which is unique to the user.</param>
            public AuthenticationResult(CSteamID npid = null)
            {
                UserID = npid;
            }

            public bool Result { get { return UserID != null; } }

            public CSteamID UserID { get; private set; }
        }

        public enum TicketValidationResult
        {
            Valid = 0,
            Invalid = 1
        }

        public IFriendsHandler FriendsHandler { get; set; }

        public interface IFriendsHandler
        {
            IEnumerable<FriendDetails> GetFriends(NPServerClient client);

            /*
            void SetFriendStatus(NPServerClient client, PresenceState presenceState,
                Dictionary<string, string> presenceData, ulong serverID)
            {
                
            }
             */
        }

        public class NPServerClient
        {
            internal NPServerClient(NPServer np, RPCServerStream rpcclient)
            {
                NP = np;
                RPC = rpcclient;
            }

            internal readonly RPCServerStream RPC;

            internal readonly NPServer NP;

            public CSteamID UserID { get; internal set; }

            public IEnumerable<FriendDetails> Friends
            {
                get { return NP.FriendsHandler.GetFriends(this).ToArray(); }
            }

            public IEnumerable<NPServerClient> FriendConnections
            {
                get { return NP.Clients.Where(c => Friends.Any(f => f.NPID == c.NPID)); }
            }

            internal NPServerClient DedicatedServer;

            private readonly Dictionary<string, string> _presence = new Dictionary<string, string>();

            public FriendsPresence[] PresenceData { get
            {
                return _presence.Select(i => new FriendsPresence {Key = i.Key, Value = i.Value}).ToArray();
            } }

            public bool IsServer { get; set; }
            public bool IsDirty { get; set; }
            public int GroupID { get; set; }

            internal void SetPresence(string key, string value)
            {
                if (!_presence.ContainsKey(key))
                    _presence.Add(key, value);
                else
                    _presence[key] = value;
            }
        }

        public enum PresenceState
        {
            Offline = 0,
            Online = 1,
            Playing = 2
        }

        public event ClientEventHandler ClientConnected;

        protected virtual void OnClientConnected(NPServerClient client)
        {
            var handler = ClientConnected;
            var args = new ClientEventArgs(client);
            if (handler != null) handler(this, args);
        }

        public event ClientEventHandler ClientDisconnected;

        protected virtual void OnClientDisconnected(NPServerClient client)
        {
            var handler = ClientDisconnected;
            var args = new ClientEventArgs(client);
            if (handler != null) handler(this, args);
        }

        public event ClientEventHandler ClientAuthenticated;

        protected virtual void OnClientAuthenticated(NPServerClient client)
        {
            var handler = ClientAuthenticated;
            var args = new ClientEventArgs(client);
            if (handler != null) handler(this, args);
        }
    }
}
