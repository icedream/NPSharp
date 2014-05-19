using NPSharp.NP;
using NPSharp.RPC.Messages.Data;

namespace NPSharp.Handlers
{
    /// <summary>
    ///     Represents a handler for all authentication-related requests.
    /// </summary>
    public interface IAuthenticationHandler
    {
        /// <summary>
        ///     Authenticates a user based on username and password.
        /// </summary>
        /// <param name="client">The NP server client to authenticate</param>
        /// <param name="username">The username to use for authentication</param>
        /// <param name="password">The password to use for authentication</param>
        /// <returns>An instance of <seealso cref="NPAuthenticationResult" /></returns>
        NPAuthenticationResult AuthenticateUser(NPServerClient client, string username, string password);

        /// <summary>
        ///     Authenticates a user based on a session token.
        /// </summary>
        /// <param name="client">The NP server client to authenticate</param>
        /// <param name="token">The session token to use for authentication</param>
        /// <returns>An instance of <seealso cref="NPAuthenticationResult" /></returns>
        NPAuthenticationResult AuthenticateUser(NPServerClient client, string token);

        /// <summary>
        ///     Authenticates a dedicated server based on its license key.
        /// </summary>
        /// <param name="client">The NP server client of the dedicated server to authenticate</param>
        /// <param name="licenseKey">The license key to use for authentication</param>
        /// <returns>An instance of <see cref="NPAuthenticationResult" /></returns>
        NPAuthenticationResult AuthenticateServer(NPServerClient client, string licenseKey);

        /// <summary>
        ///     Validates a ticket.
        /// </summary>
        /// <param name="client">The NP server client of the user who is trying to get the ticket validated</param>
        /// <param name="server">The server that the user wants to connect to using this ticket</param>
        /// <returns>A <see cref="TicketValidationResult" /> determining if the ticket is valid</returns>
        TicketValidationResult ValidateTicket(NPServerClient client, NPServerClient server);
    }
}