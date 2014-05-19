using System;
using System.Text;
using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Core;
using log4net.Layout;
using NPSharp.Authentication;
using NPSharp.NP;

namespace NPSharp.CommandLine.MOTD
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
                log.ErrorFormat("Needs 4 arguments: hostname port username password");
                return;
            }

            var hostname = args[0];
            var port = ushort.Parse(args[1]);
            var username = args[2];
            var password = args[3];

            // NP connection setup
            log.DebugFormat("Connecting to {0}:{1}...", hostname, port);
            var np = new NPClient(hostname, port);
            if (!np.Connect())
            {
                log.Error("Connection to NP server failed.");
                return;
            }

            // Get session token
            var ah = new SessionAuthenticationClient(hostname);
            try
            {
                ah.Authenticate(username, password);
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

            // Validate authentication using session token
            try
            {
                np.AuthenticateWithToken(ah.SessionToken).Wait();
            }
            catch (Exception err)
            {
#if DEBUG
                log.ErrorFormat("Authenticated but session token was invalid. {0}", err);
#else
                log.ErrorFormat("Authenticated but session token was invalid ({0}).", err.Message);
#endif
                return;
            }

            try
            {
                log.InfoFormat("Server says: {0}",
                    Encoding.UTF8.GetString(np.GetPublisherFile("motd-english.txt").Result));
                np.Disconnect();
            }
            catch
            {
                log.ErrorFormat("Could not read MOTD from NP server.");
            }
        }
    }
}