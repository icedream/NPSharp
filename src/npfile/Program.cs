using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Core;
using log4net.Layout;
using NPSharp.Authentication;
using NPSharp.NP;
using uhttpsharp;
using uhttpsharp.Handlers;
using uhttpsharp.Headers;
using uhttpsharp.Listeners;
using uhttpsharp.RequestProviders;

namespace NPSharp.CommandLine.File
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            // log4net setup
            SetupLog4Net();
            var log = LogManager.GetLogger("Main");

            // Arguments
            if (args.Length < 4)
            {
                log.ErrorFormat("Needs 4 arguments: nphostname npport username password [httpport]");
                return;
            }

            var hostname = args[0];
            var port = ushort.Parse(args[1]);
            var username = args[2];
            var password = args[3];
            var hport = args.Length > 4 ? ushort.Parse(args[4]) : 5680;

            // Get session token
            var ah = new SessionAuthenticationClient(hostname);
            try
            {
                ah.Authenticate(username, password);
                log.Info("NP authentication successful.");
            }
            catch (Exception err)
            {
#if DEBUG
                log.ErrorFormat("Could not authenticate: {0}", err);
#else
                log.ErrorFormat("Could not authenticate: {0}", err.Message);
#endif
                return;
            }

            // NP connection setup
            log.DebugFormat("Connecting to {0}:{1}...", hostname, port);
            var np = new NPClient(hostname, port);
            if (!np.Connect())
            {
                log.Error("Connection to NP server failed.");
                return;
            }
            log.Info("NP connection successful, authenticating..."); // ???
            if (!np.AuthenticateWithToken(ah.SessionToken).Result)
            {
                np.Disconnect();
                log.Error("Authentication to NP server failed.");
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
                log.Info("You can shut down the HTTP server by pressing any key.");
                Console.ReadKey();
            }
        }

        private static void SetupLog4Net()
        {
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
        }
    }
}