using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Core;
using log4net.Layout;
using NPSharp.Authentication;
using uhttpsharp;
using uhttpsharp.Handlers;
using uhttpsharp.Headers;
using uhttpsharp.Listeners;
using uhttpsharp.RequestProviders;
using HttpResponse = uhttpsharp.HttpResponse;

namespace NPSharp.CommandLine.File
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            // log4net setup
            if (Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX)
            {
                var appender = new ConsoleAppender
                {
#if DEBUG
                    Threshold = Level.Debug,
#else
                Threshold = Level.Info,
#endif
                    Layout = new PatternLayout("<%d{HH:mm:ss}> [%logger:%thread] %level: %message%newline"),
                };
                BasicConfigurator.Configure(new IAppender[]
                {appender, new DebugAppender {Layout = appender.Layout, Threshold = Level.All}});
            }
            else
            {
                var appender = new ColoredConsoleAppender
                {
#if DEBUG
                    Threshold = Level.Debug,
#else
                Threshold = Level.Info,
#endif
                    Layout = new PatternLayout("<%d{HH:mm:ss}> [%logger:%thread] %level: %message%newline"),
                };
                appender.AddMapping(new ColoredConsoleAppender.LevelColors
                {
                    Level = Level.Debug,
                    ForeColor = ColoredConsoleAppender.Colors.Cyan | ColoredConsoleAppender.Colors.HighIntensity
                });
                appender.AddMapping(new ColoredConsoleAppender.LevelColors
                {
                    Level = Level.Info,
                    ForeColor = ColoredConsoleAppender.Colors.Green | ColoredConsoleAppender.Colors.HighIntensity
                });
                appender.AddMapping(new ColoredConsoleAppender.LevelColors
                {
                    Level = Level.Warn,
                    ForeColor = ColoredConsoleAppender.Colors.Purple | ColoredConsoleAppender.Colors.HighIntensity
                });
                appender.AddMapping(new ColoredConsoleAppender.LevelColors
                {
                    Level = Level.Error,
                    ForeColor = ColoredConsoleAppender.Colors.Red | ColoredConsoleAppender.Colors.HighIntensity
                });
                appender.AddMapping(new ColoredConsoleAppender.LevelColors
                {
                    Level = Level.Fatal,
                    ForeColor = ColoredConsoleAppender.Colors.White | ColoredConsoleAppender.Colors.HighIntensity,
                    BackColor = ColoredConsoleAppender.Colors.Red
                });
                appender.ActivateOptions();
                BasicConfigurator.Configure(new IAppender[]
                {appender, new DebugAppender {Layout = appender.Layout, Threshold = Level.All}});
            }

            ILog log = LogManager.GetLogger("Main");

            // Arguments
            if (args.Length < 4)
            {
                log.ErrorFormat("Needs 4 arguments: nphostname npport username password [httpport]");
                return;
            }

            string hostname = args[0];
            ushort port = ushort.Parse(args[1]);
            string username = args[2];
            string password = args[3];
            int hport = args.Length > 4 ? ushort.Parse(args[4]) : 5680;

            // NP connection setup
            log.DebugFormat("Connecting to {0}:{1}...", hostname, port);
            var np = new NPClient(hostname, port);
            if (!np.Connect())
            {
                log.Error("Connection to NP server failed.");
                return;
            }
            log.Info("NP connection successful, authenticating...");

            // Get session token
            var ah = new SessionAuthenticationClient(hostname);
            try
            {
                ah.Authenticate(username, password);
                np.AuthenticateWithToken(ah.SessionToken).Wait();
                log.Info("NP authentication successful.");
            }
            catch (Exception err)
            {
                np.Disconnect();
#if DEBUG
                log.ErrorFormat("Could not authenticate: {0}", err);
#else
                log.ErrorFormat("Could not authenticate: {0}", err.Message);
#endif
                return;
            }

            // HTTP server
            using (var httpServer = new HttpServer(new HttpRequestProvider()))
            {
                log.Info("Starting up HTTP server...");
                httpServer.Use(new TcpListenerAdapter(new TcpListener(IPAddress.Any, hport)));
                httpServer.Use(new HttpRouter()
                    .With("user", new NP2HTTPUserFileHandler(np))
                    .With("pub", new NP2HTTPPublisherFileHandler(np))
                    );
                httpServer.Use(new AnonymousHttpRequestHandler((context, next) =>
                {
                    context.Response = new HttpResponse(HttpResponseCode.NotFound, "File not found",
                        context.Request.Headers.KeepAliveConnection());
                    return Task.Factory.GetCompleted();
                }));
                httpServer.Start();
                log.InfoFormat("HTTP server now running on port {0}.", hport);
                log.InfoFormat("Access publisher files through http://{0}:{1}/pub/<file>", IPAddress.Any, hport);
                log.InfoFormat("Access user files through http://{0}:{1}/user/<file>", IPAddress.Any, hport);
                Thread.Sleep(Timeout.Infinite);
            }
        }
    }

    internal class NP2HTTPUserFileHandler : IHttpRequestHandler
    {
        private readonly ILog _log;
        private readonly NPClient _np;

        public NP2HTTPUserFileHandler(NPClient np)
        {
            _np = np;
            _log = LogManager.GetLogger(GetType());
        }

        public Task Handle(IHttpContext context, Func<Task> next)
        {
            string uri = context.Request.QueryString.Any()
                ? null
                : string.Join("/", context.Request.Uri.OriginalString.Split('/').Skip(2));
            if (uri == null)
                if (!context.Request.QueryString.TryGetByName("uri", out uri) || uri == null)
                {
                    context.Response = HttpResponse.CreateWithMessage(HttpResponseCode.NotFound, "Invalid request",
                        context.Request.Headers.KeepAliveConnection(),
                        "You need to provide a <code>uri</code> parameter with the URL."
                        );
                    return Task.Factory.GetCompleted();
                }

            _log.InfoFormat("Requesting user file {0}", uri);
            Task<byte[]> task = _np.GetUserFile(uri);
            try
            {
                task.Wait();
            }
            catch
            {
                context.Response = HttpResponse.CreateWithMessage(HttpResponseCode.NotFound, "File not accessible",
                    context.Request.Headers.KeepAliveConnection(),
                    string.Format("<pre><tt><code>{0}</code></tt></pre>",
                        task.Exception == null ? "Unknown error" : task.Exception.ToString())
                    );
                return Task.Factory.GetCompleted();
            }

            // Return file contents
            context.Response = new HttpResponse(HttpResponseCode.Ok, MimeMapping.GetMimeMapping(uri),
                new MemoryStream(task.Result), context.Request.Headers.KeepAliveConnection());

            return Task.Factory.GetCompleted();
        }
    }

    internal class NP2HTTPPublisherFileHandler : IHttpRequestHandler
    {
        private readonly ILog _log;
        private readonly NPClient _np;

        public NP2HTTPPublisherFileHandler(NPClient np)
        {
            _np = np;
            _log = LogManager.GetLogger(GetType());
        }

        public Task Handle(IHttpContext context, Func<Task> next)
        {
            string uri = context.Request.QueryString.Any()
                ? null
                : string.Join("/", context.Request.Uri.OriginalString.Split('/').Skip(2));
            if (uri == null)
                if (!context.Request.QueryString.TryGetByName("uri", out uri) || uri == null)
                {
                    context.Response = HttpResponse.CreateWithMessage(HttpResponseCode.NotFound, "Invalid request",
                        context.Request.Headers.KeepAliveConnection(),
                        "You need to provide a <code>uri</code> parameter with the URL."
                        );
                    return Task.Factory.GetCompleted();
                }

            _log.InfoFormat("Requesting publisher file {0}", uri);
            Task<byte[]> task = _np.GetPublisherFile(uri);
            try
            {
                task.Wait();
            }
            catch
            {
                context.Response = HttpResponse.CreateWithMessage(HttpResponseCode.NotFound, "File not accessible",
                    context.Request.Headers.KeepAliveConnection(),
                    string.Format("<pre><tt><code>{0}</code></tt></pre>",
                        task.Exception == null ? "Unknown error" : task.Exception.ToString())
                    );
                return Task.Factory.GetCompleted();
            }

            // Return file contents
            context.Response = new HttpResponse(HttpResponseCode.Ok, MimeMapping.GetMimeMapping(uri),
                new MemoryStream(task.Result), context.Request.Headers.KeepAliveConnection());

            return Task.Factory.GetCompleted();
        }
    }
}