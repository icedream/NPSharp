using System.Collections.Generic;
using System.IO;

using Newtonsoft.Json.Linq;

namespace NPSharp.RPC.Messages.Data
{
    /// <summary>
    ///     Represents the outcome of a ticket validation attempt, including eventual NPv2 authentication identifiers passed by the server.
    /// </summary>
    public class TicketValidationResult
    {
        internal TicketValidationResult(Server.AuthenticateValidateTicketResultMessage message)
        {
            IsValid = message.Result == 0;

            Identifiers = ParseIdentifierList(message.Identifiers);
        }

        internal IEnumerable<string> ParseIdentifierList(string serializedList)
        {
            // current layer1 implementation uses JSON, formatted as `[ [ <type>, <value> ]... ]` - we'll concatenate these as strings to prevent this implementation detail
            // this is consistent with the external API for `profiles` in Citizen itself
            JToken jsonValue;

            try
            {
                jsonValue = JToken.Parse(serializedList);
            }
            catch (FileLoadException)
            {
                return new string[0];
            }

            var identifiers = new List<string>();

            if (jsonValue.Type == JTokenType.Array)
            {
                var array = (JArray)jsonValue;

                foreach (var identifierToken in array.Children())
                {
                    if (identifierToken.Type == JTokenType.Array)
                    {
                        var identifierArray = (JArray)identifierToken;

                        identifiers.Add(string.Format("{0}:{1}", identifierArray[0], identifierArray[1]));
                    }
                }
            }

            return identifiers;
        }

        /// <summary>
        /// Whether the ticket is valid or not.
        /// </summary>
        public bool IsValid { get; private set; }

        /// <summary>
        /// A list of NPv2 authentication identifiers belonging to the ticket session.
        /// </summary>
        public IEnumerable<string> Identifiers { get; private set; }
    }
}