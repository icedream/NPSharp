using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using log4net;
using uhttpsharp;
using uhttpsharp.Handlers;
using uhttpsharp.Headers;
using uhttpsharp.Listeners;
using uhttpsharp.RequestProviders;

namespace NPSharp.Authentication
{
    /// <summary>
    /// Represents a session authentication server which uses the HTTP protocol to send out session tokens to authenticating NP clients.
    /// </summary>
    public class SessionAuthenticationServer
    {
        private readonly ILog _log;
        private HttpServer _http;

        /// <summary>
        /// Constructs a new session authentication server.
        /// </summary>
        public SessionAuthenticationServer()
        {
            SupportOldAuthentication = true;
            _log = LogManager.GetLogger("Auth");
        }

        /// <summary>
        /// Support oldskool "user&amp;&amp;pass" authentication format.
        /// </summary>
        public bool SupportOldAuthentication { get; set; }

        /// <summary>
        /// Will be triggered whenever a client tries to authenticate via this server.
        /// </summary>
        public event Func<string, string, SessionAuthenticationResult> Authenticating;

        protected virtual SessionAuthenticationResult OnAuthenticating(string username, string password)
        {
            var handler = Authenticating;
            return handler != null
                ? handler(username, password)
                : new SessionAuthenticationResult {Reason = "Login currently disabled"};
        }

        /// <summary>
        /// Starts the authentication server.
        /// </summary>
        /// <param name="port">The port on which the authentication server should listen on.</param>
        public void Start(ushort port = 12003)
        {
            if (_http != null)
            {
                throw new InvalidOperationException("This server is already running");
            }

            _log.Debug("Starting authentication server...");
            _http = new HttpServer(new HttpRequestProvider());
            _http.Use(new TcpListenerAdapter(new TcpListener(IPAddress.Any, port)));
            _http.Use(new TcpListenerAdapter(new TcpListener(IPAddress.IPv6Any, port)));
            _http.Use(new HttpRouter().With("authenticate", new AuthenticateHandler(this)));
            _http.Use(new AnonymousHttpRequestHandler((ctx, task) =>
            {
                ctx.Response = HttpResponse.CreateWithMessage(HttpResponseCode.NotFound, "Not found",
                    ctx.Request.Headers.KeepAliveConnection());
                return Task.Factory.GetCompleted();
            }));
            _http.Start();
            _log.Debug("Done starting authentication server.");
        }

        /// <summary>
        /// Stops the authentication server.
        /// </summary>
        public void Stop()
        {
            _http.Dispose();
        }

        protected class AuthenticateHandler : IHttpRequestHandler
        {
            private readonly SessionAuthenticationServer _authServer;

            public AuthenticateHandler(SessionAuthenticationServer sessionAuthenticationServer)
            {
                _authServer = sessionAuthenticationServer;
            }

            public Task Handle(IHttpContext context, Func<Task> next)
            {
                SessionAuthenticationResult sar;

                string username;
                string password;

                if (!context.Request.Post.Parsed.TryGetByName("user", out username) ||
                    !context.Request.Post.Parsed.TryGetByName("pass", out password))
                {
                    var login = Encoding.UTF8.GetString(context.Request.Post.Raw)
                        .Split(new[] {"&&"}, StringSplitOptions.None);
                    if (login.Length != 2)
                    {
                        sar = new SessionAuthenticationResult {Reason = @"Invalid login data"};

                        context.Response = new HttpResponse(HttpResponseCode.Ok, sar.ToString(),
                            context.Request.Headers.KeepAliveConnection());
                        return Task.Factory.GetCompleted();
                    }

                    username = login[0];
                    password = login[1];
                }

                try
                {
                    sar = _authServer.OnAuthenticating(username, password);
                }
                catch (Exception error)
                {
                    _authServer._log.Error(@"Authentication handler error", error);
                    sar = new SessionAuthenticationResult {Reason = @"Internal server error"};
                }

                context.Response = new HttpResponse(HttpResponseCode.Ok, sar.ToString(),
                    !sar.Success && context.Request.Headers.KeepAliveConnection());
                return Task.Factory.GetCompleted();
            }
        }
    }

    public class SessionAuthenticationResult
    {
        /// <summary>
        /// true if authentication was successful, otherwise false.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Reason for the given success state. Use this especially in authentication fail cases.
        /// </summary>
        public string Reason { get; set; }

        /// <summary>
        /// If authenticated set this to the user's unique ID.
        /// </summary>
        public uint UserID { get; set; }

        /// <summary>
        /// If authenticated set this to the user's session token.
        /// </summary>
        public string SessionToken { get; set; }

        /// <summary>
        /// If authenticated set this to the actual correctly spelled username.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// If authenticated set this to the user's e-mail address.
        /// </summary>
        public string UserMail { get; set; }

        /// <summary>
        /// Returns the response line as it should be sent out to the client.
        /// </summary>
        public override string ToString()
        {
            return String.Join("#",
                Success ? "ok" : "fail",
                String.IsNullOrEmpty(Reason) ? (Success ? "Success" : "Unknown error") : Reason,
                UserID,
                UserName,
                UserMail,
                SessionToken,
                String.Empty);
        }
    }
}