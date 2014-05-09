using System;
using System.Threading;
using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Core;
using log4net.Layout;
using NPSharp.Authentication;

namespace NPSharp.CommandLine.Server
{
    class Program
    {
        static void Main()
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
                BasicConfigurator.Configure(new IAppender[] { appender, new DebugAppender { Layout = appender.Layout, Threshold = Level.All } });
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
                BasicConfigurator.Configure(new IAppender[] { appender, new DebugAppender { Layout = appender.Layout, Threshold = Level.All } });
            }

            var log = LogManager.GetLogger("Main");

            log.Info("Now starting authentication server...");
            var auth = new SessionAuthenticationServer();
            auth.Authenticating += (user, pw) => new SessionAuthenticationResult()
            {
                SessionToken = Guid.NewGuid().ToString("N"),
                UserID = 1,
                Success = true,
                UserMail = "anonymous@localhost",
                UserName = "anonymous"
            };
            auth.Start();
            log.Info("Authentication server started up successfully.");

            log.Info("Now starting NP server...");
            var np = new NPServer(3036)
            {
                AuthenticationHandler = new DummyAuthenticationHandler(),
                FileServingHandler = new DummyFileServingHandler()
                // TODO: Implement the other handlers
            };
            np.Start();
            log.Info("NP server started up successfully.");

            Thread.Sleep(Timeout.Infinite);
        }
    }
}
