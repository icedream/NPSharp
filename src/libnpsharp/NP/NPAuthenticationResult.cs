using NPSharp.Steam;

namespace NPSharp.NP
{
    /// <summary>
    ///     Represents details about the outcome of an authentication attempt.
    /// </summary>
    public class NPAuthenticationResult
    {
        /// <summary>
        ///     Constructs an authentication result instance.
        /// </summary>
        /// <param name="npid">
        ///     Set this to null if authentication should fail, otherwise use an instance of a steam ID which is
        ///     unique to the user.
        /// </param>
        public NPAuthenticationResult(CSteamID npid = null)
        {
            UserID = npid;
        }

        /// <summary>
        ///     True if authentiation succeeded, otherwise false.
        /// </summary>
        public bool Result
        {
            get { return UserID != null; }
        }

        /// <summary>
        ///     The assigned user ID by the authentication provider. Can be null for failed authentication attempts.
        /// </summary>
        public CSteamID UserID { get; private set; }
    }
}