using System;

namespace NPSharp.Authentication
{
    public class SessionAuthenticationResult
    {
        /// <summary>
        ///     true if authentication was successful, otherwise false.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        ///     Reason for the given success state. Use this especially in authentication fail cases.
        /// </summary>
        public string Reason { get; set; }

        /// <summary>
        ///     If authenticated set this to the user's unique ID.
        /// </summary>
        public uint UserID { get; set; }

        /// <summary>
        ///     If authenticated set this to the user's session token.
        /// </summary>
        public string SessionToken { get; set; }

        /// <summary>
        ///     If authenticated set this to the actual correctly spelled username.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        ///     If authenticated set this to the user's e-mail address.
        /// </summary>
        public string UserMail { get; set; }

        /// <summary>
        ///     Returns the response line as it should be sent out to the client.
        /// </summary>
        public override string ToString()
        {
            // Response will be in this syntax:
            // (ok|fail)#text#userid#username#email#sessiontoken
            return String.Join("#",
                Success ? "ok" : "fail",
                String.IsNullOrEmpty(Reason) ? (Success ? "Success" : "Unknown error") : Reason,
                UserID,
                string.IsNullOrEmpty(UserName) ? "Anonymous" : UserName,
                string.IsNullOrEmpty(UserMail) ? "anonymous@localhost" : UserMail,
                string.IsNullOrEmpty(SessionToken) ? "0" : SessionToken,
                String.Empty);
        }
    }
}