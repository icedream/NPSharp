using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using NPSharp.Master.Messages.Data;

namespace NPSharp.Master.Messages.Client
{
    /// <summary>
    /// Represents a request message for the master server for an extended dedicated server list.
    /// </summary>
    [MasterClientMessage("getserversExt")]
    public class MasterGetServersExtendedMessage : MasterClientMessage
    {
        private static readonly ILog Log;

        static MasterGetServersExtendedMessage()
        {
            Log = LogManager.GetLogger(typeof (MasterGetServersExtendedMessage));
        } 

        protected override string Serialize()
        {
            // I wonder if an extra useless space char at the end is okay in this case
            return string.Format("{0} {1} {2} {3}", Name, GameName, ProtocolVersion, string.Join(" ", Keywords.Select(k => k.ToString())));
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
                        Keywords.Add(MasterGetServersExtendedKeywords.Empty);
                        break;
                    case "full":
                        Keywords.Add(MasterGetServersExtendedKeywords.Full);
                        break;
                    case "ipv4":
                        Keywords.Add(MasterGetServersExtendedKeywords.InternetProtocolVersion4);
                        break;
                    case "ipv6":
                        Keywords.Add(MasterGetServersExtendedKeywords.InternetProtocolVersion6);
                        break;
                    default:
                        Log.WarnFormat("{0}: weird keyword {1}", Name, kw);
                        break;
                }
            }
        }

        /// <summary>
        /// The game for which servers should be fetched
        /// </summary>
        public string GameName { get; set; }

        /// <summary>
        /// The protocol version of the dedicated servers to search for
        /// </summary>
        public uint ProtocolVersion { get; set; }

        /// <summary>
        /// Extra keywords to take care of when generating the server list
        /// </summary>
        public List<MasterGetServersExtendedKeywords> Keywords { get; set; }
    }
}

namespace NPSharp.Master.Messages.Data
{
    /// <summary>
    /// Represents keywords for a master server standard serverlist request.
    /// </summary>
    public enum MasterGetServersExtendedKeywords
    {
        Full = 0x01,
        Empty = 0x02,
        InternetProtocolVersion4 = 0x04,
        InternetProtocolVersion6 = 0x08
    }
}