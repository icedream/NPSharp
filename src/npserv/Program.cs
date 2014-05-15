using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Core;
using log4net.Layout;
using NPSharp.Authentication;
using NPSharp.CommandLine.Server.Database;

namespace NPSharp.CommandLine.Server
{
    internal class Program
    {
        private static ILog _log;
        private static SessionAuthenticationServer _authServer;
        private static NPServer _np;

        private static void Main()
        {
            InitializeLogging();
            _log.Info("NP server is about to start up, this might take a few seconds...");

            InitializeDatabase();
            InitializeAuthServer();
            InitializeNPServer();

            _log.Info("NP server started up successfully.");
            Thread.Sleep(Timeout.Infinite);
        }

        private static BrightstarDatabaseContext OpenDatabase(string store = "NP")
        {
            // TODO: This line is CREATING a new database but it's supposed to open it only if it's already created. Look up!
            return
                new BrightstarDatabaseContext(
                    "type=embedded;storesdirectory=Database\\;storename=" + store,
                    true);
        }

        private static void InitializeDatabase()
        {
            _log.Debug("Preparing database...");

            using (var db = OpenDatabase())
            {
                // Skip user creation if there are already registered users

                // ReSharper disable once UseMethodAny.0
                // since SPARQL-to-LINQ does not have support for Any() yet
                if (db.Users.Count() > 0)
                    return;

                // Create first user (test:test)
                var testUser = db.Users.Create();
                testUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword("test");
                testUser.UserMail = "test@localhost";
                testUser.UserName = "test";

                _log.InfoFormat(
                    "Created first user with following details:" + Environment.NewLine + Environment.NewLine +
                    "Username: {0}" + Environment.NewLine + "Password: {1}",
                    testUser.UserName,
                    "test");

                db.SaveChanges();

                _log.DebugFormat("First user id is {0}", testUser.Id);
            }

            // Cleanup thread
            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    using (var dbForCleanup = OpenDatabase())
                    {
                        _log.Debug("Starting cleanup...");
                        foreach (var session in dbForCleanup.Sessions.Where(s => s.ExpiryTime < DateTime.Now).ToArray())
                        {
                            _log.DebugFormat("Session {0} became invalid", session.Id);
                            dbForCleanup.DeleteObject(session);
                        }
                        foreach (var ban in dbForCleanup.Bans.Where(s => s.ExpiryTime < DateTime.Now).ToArray())
                        {
                            _log.DebugFormat("Ban {0} became invalid", ban.Id);
                            dbForCleanup.DeleteObject(ban);
                        }

                        foreach (var cheatDetection in dbForCleanup.CheatDetections.Where(s => s.ExpiryTime < DateTime.Now).ToArray())
                        {
                            _log.DebugFormat("Cheat detection {0} became invalid", cheatDetection.Id);
                            dbForCleanup.DeleteObject(cheatDetection);
                        }

                        _log.Debug("Saving cleanup...");
                        dbForCleanup.SaveChanges();

                        _log.Debug("Cleanup done.");
                    }

                    Thread.Sleep(TimeSpan.FromSeconds(30));
                }

                // TODO: implement some way to cancel this loop
                // ReSharper disable once FunctionNeverReturns
            });
        }

        private static void InitializeAuthServer()
        {
            _log.Debug("Starting authentication server...");
            _authServer = new SessionAuthenticationServer();
            _authServer.Authenticating += (loginUsername, loginPassword) =>
            {
                using (var db = OpenDatabase())
                {
                    var matchingUsers =
                        db.Users.Where(u => u.UserName == loginUsername).ToArray() // brightstar level
                        .Where(u => BCrypt.Net.BCrypt.Verify(loginPassword, u.PasswordHash)).ToArray() // local level
                        ;

                    if (!matchingUsers.Any())
                        return new SessionAuthenticationResult {Reason = "Invalid credentials"};

                    var user = matchingUsers.Single();

                    // Check for bans
                    var bans = user.Bans.Where(b => b.ExpiryTime > DateTime.Now).ToArray();
                    if (bans.Any())
                    {
                        var ban = bans.First();
                        return new SessionAuthenticationResult
                        {
                            Reason = string.Format("You're banned: {0} (until {1})", ban.Reason, ban.ExpiryTime)
                        };
                    }

                    // Check for cheat detections
                    var cheatDetections =
                        user.CheatDetections.Where(c => c.ExpiryTime > DateTime.Now).ToArray();
                    if (cheatDetections.Any())
                    {
                        var cheatDetection = cheatDetections.First();
                        return new SessionAuthenticationResult
                        {
                            Reason =
                                string.Format("Detected cheat #{0}: {1} (until {2})", cheatDetection.CheatId,
                                    cheatDetection.Reason, cheatDetection.ExpiryTime)
                        };
                    }

                    // Create user session
                    var session = db.Sessions.Create();
                    session.ExpiryTime = DateTime.Now + TimeSpan.FromMinutes(3);
                    user.Sessions.Add(session);

                    // Update user's last login data
                    user.LastLogin = DateTime.Now;

                    // Save to database
                    db.SaveChanges();

                    // Return session information
                    return new SessionAuthenticationResult
                    {
                        Success = true,
                        SessionToken = session.Id,
                        UserID = uint.Parse(user.Id, NumberStyles.Integer),
                        UserMail = user.UserMail,
                        UserName = user.UserName
                    };
                }
            }
                ;
            _authServer.Start();
        }

        private static void InitializeNPServer()
        {
            _log.Debug("Starting NP server...");
            _np = new NPServer(3036)
            {
                AuthenticationHandler = new BrightstarDatabaseAuthenticationHandler(OpenDatabase()),
                FileServingHandler = new BrightstarDatabaseFileServingHandler(OpenDatabase()),
                FriendsHandler = null,
                UserAvatarHandler = null
            };
            _np.Start();
        }

        private static void InitializeLogging()
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

                BasicConfigurator.Configure(
                    new IAppender[]
                    {
                        appender,
                        new DebugAppender {Layout = appender.Layout, Threshold = Level.All}
                    });
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

                appender.AddMapping(
                    new ColoredConsoleAppender.LevelColors
                    {
                        Level = Level.Debug,
                        ForeColor = ColoredConsoleAppender.Colors.Cyan | ColoredConsoleAppender.Colors.HighIntensity
                    });
                appender.AddMapping(
                    new ColoredConsoleAppender.LevelColors
                    {
                        Level = Level.Info,
                        ForeColor =
                            ColoredConsoleAppender.Colors.Green | ColoredConsoleAppender.Colors.HighIntensity
                    }
                    );

                appender.AddMapping(
                    new ColoredConsoleAppender.LevelColors
                    {
                        Level = Level.Warn,
                        ForeColor =
                            ColoredConsoleAppender.Colors.Purple | ColoredConsoleAppender.Colors.HighIntensity
                    });

                appender.AddMapping(
                    new ColoredConsoleAppender.LevelColors
                    {
                        Level = Level.Error,
                        ForeColor = ColoredConsoleAppender.Colors.Red | ColoredConsoleAppender.Colors.HighIntensity
                    }
                    );
                appender.AddMapping(
                    new ColoredConsoleAppender.LevelColors
                    {
                        Level = Level.Fatal,
                        ForeColor =
                            ColoredConsoleAppender.Colors.White | ColoredConsoleAppender.Colors.HighIntensity,
                        BackColor = ColoredConsoleAppender.Colors.Red
                    });

                appender.ActivateOptions();
                BasicConfigurator.Configure(
                    new IAppender[]
                    {
                        appender,
                        new DebugAppender {Layout = appender.Layout, Threshold = Level.All}
                    });
            }

            _log = LogManager.GetLogger("Main");
        }
    }
}