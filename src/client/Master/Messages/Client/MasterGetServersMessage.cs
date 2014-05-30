using System.Collections.Generic;
using System.Linq;
using log4net;
using NPSharp.Master.Data;

namespace NPSharp.Master.Messages.Client
{
    /// <summary>
    ///     Represents a request message for the master server for a standard dedicated server list.
    /// </summary>
    [MasterClientMessage("getservers")]
    public class MasterGetServersMessage : MasterClientMessage
    {
        private static readonly ILog Log;

        static MasterGetServersMessage()
        {
            Log = LogManager.GetLogger(typeof (MasterGetServersMessage));
        }

        /// <summary>
        ///     The game for which servers should be fetched
        /// </summary>
        public string GameName { get; set; }

        /// <summary>
        ///     The protocol version of the dedicated servers to search for
        /// </summary>
        public uint ProtocolVersion { get; set; }

        /// <summary>
        ///     Extra keywords to take care of when generating the server list
        /// </summary>
        public List<MasterGetServersKeywords> Keywords { get; set; }

        protected override string Serialize()
        {
            // I wonder if an extra useless space char at the end is okay in this case
            return string.Format("{0} {1} {2} {3}", Name, GameName, ProtocolVersion,
                string.Join(" ", Keywords.Select(k => k.ToString())));
        }

        protected override void Deserialize(string[] arguments)
        {
            GameName = arguments[0];
            ProtocolVersion = uint.Parse(arguments[1]);
            foreach (var kw in arguments.Skip(2))
            {
                switch (kw.ToLower())
                {
                    case "empty":
                        Keywords.Add(MasterGetServersKeywords.Empty);
                        break;
                    case "full":
                        Keywords.Add(MasterGetServersKeywords.Full);
                        break;
                    default:
                        Log.WarnFormat("{0}: weird keyword {1}", Name, kw);
                        break;
                }
            }
        }
    }
}