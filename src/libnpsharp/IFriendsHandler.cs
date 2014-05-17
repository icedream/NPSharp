using System.Collections.Generic;
using NPSharp.RPC.Messages;
using NPSharp.RPC.Messages.Structs;

namespace NPSharp
{
    /// <summary>
    ///     Represents a handler for all friends-related requests.
    /// </summary>
    public interface IFriendsHandler
    {
        /// <summary>
        ///     Fetches all friends of the connected user.
        /// </summary>
        /// <param name="client">The NP server client of the user</param>
        /// <returns>All friend details found for the user</returns>
        IEnumerable<FriendDetails> GetFriends(NPServerClient client);
    }
}