using System.Collections.Generic;
using System.Linq;
using NPSharp.RPC;
using NPSharp.RPC.Messages;
using NPSharp.Steam;

namespace NPSharp
{
    /// <summary>
    ///     Represents a remote client connection to an NP server.
    /// </summary>
    public class NPServerClient
    {
        internal readonly NPServer NP;
        internal readonly RPCServerStream RPC;
        private readonly Dictionary<string, string> _presence = new Dictionary<string, string>();
        internal NPServerClient DedicatedServer;

        internal NPServerClient(NPServer np, RPCServerStream rpcclient)
        {
            NP = np;
            RPC = rpcclient;
        }

        public CSteamID UserID { get; internal set; }

        public IEnumerable<FriendDetails> Friends
        {
            get
            {
                return NP.FriendsHandler == null
                    ? new FriendDetails[0]
                    : NP.FriendsHandler.GetFriends(this).ToArray();
            }
        }

        public IEnumerable<NPServerClient> FriendConnections
        {
            get { return NP.Clients.Where(c => Friends.Any(f => f.NPID == c.UserID)); }
        }

        public FriendsPresence[] PresenceData
        {
            get { return _presence.Select(i => new FriendsPresence {Key = i.Key, Value = i.Value}).ToArray(); }
        }

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
}