using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace NPSharp.Authentication
{
    /// <summary>
    ///     Represents a client which can communicate with an authentication endpoint in order to retrieve session
    ///     information, including tokens for authentication with NP servers.
    /// </summary>
    public class SessionAuthenticationClient
    {
        private readonly string _host;
        private readonly string _path;
        private readonly ushort _port;

        /// <summary>
        ///     Initializes a new instance of the <see cref="NPSharp.Authentication.SessionAuthenticationClient" /> class.
        /// </summary>
        /// <param name="host">Hostname of the authentication endpoint.</param>
        /// <param name="port">Port of the authentication endpoint.</param>
        /// <param name="path">Path of the authentication endpoint.</param>
        public SessionAuthenticationClient(string host, ushort port = 12003, string path = "/authenticate")
        {
            _host = host;
            _port = port;
            _path = path;
        }

        /// <summary>
        ///     Gets the username.
        /// </summary>
        /// <value>The username.</value>
        public string Username { get; private set; }

        /// <summary>
        ///     Gets the user's e-mail address.
        /// </summary>
        /// <value>The user's e-mail address.</value>
        public string UserEMail { get; private set; }

        /// <summary>
        ///     Gets the session token.
        /// </summary>
        /// <value>The session token.</value>
        public string SessionToken { get; private set; }

        /// <summary>
        ///     Gets the user identifier.
        /// </summary>
        /// <value>The user identifier.</value>
        public uint UserId { get; private set; }

        /// <summary>
        ///     Authenticate the specified username and password.
        /// </summary>
        /// <param name="username">The username to use for authentication.</param>
        /// <param name="password">The password to use for authentication.</param>
        public void Authenticate(string username, string password)
        {
            string post = string.Format("{0}&&{1}", username, password);

            Uri uri = new UriBuilder
            {
                Scheme = "http",
                Port = _port,
                Host = _host,
                Path = _path
            }.Uri;

            var req = (HttpWebRequest) WebRequest.Create(uri);
            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";
            req.AllowAutoRedirect = true;
            req.KeepAlive = false;
            using (Stream reqStream = req.GetRequestStream())
            {
                byte[] buffer = Encoding.UTF8.GetBytes(post);
                reqStream.Write(buffer, 0, post.Length);
                reqStream.Flush();
            }

            // Response will be in this syntax:
            // (ok|fail)#text#userid#username#email#sessiontoken
            var rx =
                new Regex(
                    "^(?<status>ok|fail)#(?<text>.+)#(?<userid>[0-9]+)#(?<username>.+)#(?<usermail>.+)#(?<sessiontoken>[^#]+)[#]*$");
            var resp = (HttpWebResponse) req.GetResponse();
            using (Stream respStream = resp.GetResponseStream())
            {
                if (respStream == null)
                    throw new Exception(@"No answer from server");
                using (var respReader = new StreamReader(respStream))
                {
                    while (!respReader.EndOfStream)
                    {
                        string line = respReader.ReadLine();

                        // No answer?
                        if (string.IsNullOrEmpty(line))
                            continue;

                        // DW response line found?
                        if (!rx.IsMatch(line))
                            continue;

                        // This is a DW response line, analyze
                        Match rxm = rx.Match(line);

                        // Login succeeded?
                        if (rxm.Groups["status"].Value != "ok")
                            throw new Exception(rxm.Groups["text"].Value);

                        // Store all data
                        Username = rxm.Groups["username"].Value;
                        UserEMail = rxm.Groups["usermail"].Value;
                        SessionToken = rxm.Groups["sessiontoken"].Value;
                        UserId = uint.Parse(rxm.Groups["userid"].Value);
                    }
                }
            }
        }
    }
}