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
    public class SessionAuthenticationServer
    {
        private HttpServer _http;

        private readonly ILog _log;

        public SessionAuthenticationServer()
        {
            SupportOldAuthentication = true;
            _log = LogManager.GetLogger("Auth");
        }

        public event Func<string, string, SessionAuthenticationResult> Authenticating;

        protected virtual SessionAuthenticationResult OnAuthenticating(string username, string password)
        {
            var handler = Authenticating;
            return handler != null ? handler(username, password) : new SessionAuthenticationResult { Reason = "Login currently disabled" };
        }

        public bool SupportOldAuthentication { get; set; }

        public void Start(ushort port = 12003)
        {
            if (_http != null)
            {
                throw new InvalidOperationException("This server is already running");
            }
            _http = new HttpServer(new HttpRequestProvider());
            _http.Use(new TcpListenerAdapter(new TcpListener(IPAddress.IPv6Any, port)));
            _http.Use(new HttpRouter().With("authenticate", new AuthenticateHandler(this)));
            _http.Use(new AnonymousHttpRequestHandler((ctx, task) =>
            {
                ctx.Response = HttpResponse.CreateWithMessage(HttpResponseCode.NotFound, "Not found", ctx.Request.Headers.KeepAliveConnection());
                return Task.Factory.GetCompleted();
            }));
            _http.Start();
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

                var login = Encoding.UTF8.GetString(context.Request.Post.Raw)
                    .Split(new[] {"&&"}, StringSplitOptions.None);
                if (login.Length != 2)
                {
                    sar = new SessionAuthenticationResult{Reason = @"Invalid login data"};
                }
                else
                {
                    try
                    {
                        sar = _authServer.OnAuthenticating(login[0], login[1]);
                    }
                    catch (Exception error)
                    {
                        _authServer._log.Error(@"Authentication handler error", error);
                        sar = new SessionAuthenticationResult { Reason = @"Internal server error" };
                    }
                }

                context.Response = new HttpResponse(HttpResponseCode.Ok, sar.ToString(), context.Request.Headers.KeepAliveConnection());
                return Task.Factory.GetCompleted();
            }
        }

        public void Stop()
        {
            _http.Dispose();
        }
    }

    public class SessionAuthenticationResult
    {
        public bool Success { get; set; }
        public string Reason { get; set; }
        public uint UserID { get; set; }
        public string SessionToken { get; set; }
        public string UserName { get; set; }
        public string UserMail { get; set; }
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
