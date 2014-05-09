using System;
using System.Collections.Generic;
using System.IO;
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
using Raven.Client;
using Raven.Client.Embedded;
using Raven.Client.Linq;
using Raven.Database.Server;

namespace NPSharp.CommandLine.Server
{
    class Program
    {
        private static ILog _log;
        private static IDocumentStore _database;
        private static IDocumentSession _db;
        private static SessionAuthenticationServer _authServer;
        private static NPServer _np;

        static void Main()
        {
            InitializeLogging();
            _log.Info("NP server is about to start up, this might take a few seconds...");

            InitializeDatabase();
            InitializeAuthServer();
            InitializeNPServer();

            _log.Info("NP server started up successfully.");
            Thread.Sleep(Timeout.Infinite);
        }

        static void InitializeDatabase()
        {
            _log.Debug("Starting Raven database...");

#if DEBUG
            Directory.CreateDirectory(@"Raven");
            Directory.CreateDirectory(@"Raven\CompiledIndexCache");
#endif
            Directory.CreateDirectory(@"Database");

            NonAdminHttp.EnsureCanListenToWhenInNonAdminContext(12002);
            var database = new EmbeddableDocumentStore
            {
                DataDirectory = "Database",
                UseEmbeddedHttpServer = true
            };
            database.Configuration.Port = 12002;
            database.Configuration.AllowLocalAccessWithoutAuthorization = true;
            database.Configuration.DataDirectory = "Database";
            _database = database.Initialize();

            _database.Conventions.IdentityTypeConvertors.Add(new UInt32Converter());

            // Set up initial admin user
            _db = _database.OpenSession();
            //using (var db = _database.OpenSession())
            //{
                if (!_db.Query<User>().Any())
                {
                    _log.Warn("Creating default admin user because no users could be found in the database...");
                    var adminUser = new User()
                    {
                        BanIDs = new List<string>(),
                        CheatDetectionIDs = new List<string>(),
                        FriendIDs = new List<uint>(),
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("test"),
                        UserMail = "admin@localhost",
                        UserName = "admin"
                    };
                    _db.Store(adminUser);
                    _db.SaveChanges();
                    _log.Warn("Default admin user created. For details see below.");
                    _log.Warn("\tUsername: admin");
                    _log.Warn("\tPassword: test");
                    _log.Warn("This only happens when no users can be found in the database. Change the details or create a new user to access the server asap!");
                }
            //}

            // Endless loop to clean up expired stuff
            Task.Factory.StartNew(() =>
            {
                while (_database != null && !_database.WasDisposed)
                {
                    using (var db = _database.OpenSession())
                    {
                        var expiredSessions = db.Query<Session>().Where(s => !s.IsValid).ToArray();
                        foreach (var session in expiredSessions)
                            db.Delete(session);

                        var expiredBans = db.Query<Ban>().Where(b => !b.IsValid).ToArray();
                        foreach (var ban in expiredBans)
                            db.Delete(ban);

                        var expiredCheatDetections = db.Query<CheatDetection>().Where(cd => !cd.IsValid).ToArray();
                        foreach (var cd in expiredCheatDetections)
                            db.Delete(cd);

                        _log.DebugFormat(
                            "Purging {0} invalid sessions, {1} invalid bans and {2} invalid cheat detections",
                            expiredSessions.Length,
                            expiredBans.Length,
                            expiredCheatDetections.Length);

                        db.SaveChanges();
                    }

                    Thread.Sleep(TimeSpan.FromMinutes(5));
                }
            });
        }

        static void InitializeAuthServer()
        {
            _log.Debug("Starting authentication server...");
            _authServer = new SessionAuthenticationServer();
            _authServer.Authenticating += (loginUsername, loginPassword) =>
            {
                //using (var db = _database.OpenSession())
                //{
                    var resp = new SessionAuthenticationResult();
                 
                    // Check if we have any user to which the given credentials fit
                    var users = _db
                        // database processing
                        .Query<User>()
                        .Customize(x => x
                            .Include<User>(o => o.BanIDs)
                            .Include<User>(o => o.CheatDetectionIDs))
                        .Where(u => u.UserName == loginUsername)
                        .ToArray()

                        // local processing
                        .Where(u => u.ComparePassword(loginPassword))
                        .ToArray();
                    if (!users.Any())
                    {
                        resp.Reason =
                            "Login details are incorrect. Please check your username and password and try again.";
                        return resp;
                    }
                    var user = users.Single();

                    // Check if user is banned
                    var bans = _db.Load<Ban>(user.BanIDs);
                    if (bans.Any(b => b.IsValid))
                    {
                        var ban = bans.First(b => b.IsValid);
                        resp.Reason = string.Format("You're currently banned: {0} (expires in {1})", ban.Reason,
                            ban.ExpiresIn.ToString("g")); // TODO: Format as d days h hours m minutes and s seconds
                        return resp;
                    }

                    // Check if user was hacking
                    var cheatDetections = _db.Load<CheatDetection>(user.CheatDetectionIDs);
                    if (cheatDetections.Any(b => b.IsValid))
                    {
                        var ban = cheatDetections.First(b => b.IsValid);
                        resp.Reason = string.Format("You have been seen using a cheat: {0} (expires in {1})", ban.Reason,
                            ban.ExpiresIn.ToString("g")); // TODO: Format as d days h hours m minutes and s seconds
                        return resp;
                    }

                    // Create a session for this user
                    var session = new Session()
                    {
                        ExpiryTime = DateTime.Now + TimeSpan.FromMinutes(5),
                        User = user
                    };
                    _db.Store(session);

                    // Update user's last login time
                    user.LastLogin = DateTime.Now;
                    _db.Store(user);

                    resp.UserID = user.Id;
                    resp.UserMail = user.UserMail;
                    resp.UserName = user.UserName;
                    resp.SessionToken = session.Id;
                    resp.Success = true;

                    // Save everything to the database now
                    _db.SaveChanges();

                    return resp;
                //}
            };
            _authServer.Start();
        }

        private static void InitializeNPServer()
        {
            _log.Debug("Starting NP server...");
            _np = new NPServer(3036)
            {
                AuthenticationHandler = new RavenDatabaseAuthenticationHandler(_database),
                FileServingHandler = new RavenDatabaseFileServingHandler(_database)
                // TODO: Implement the other handlers
            };
            _np.Start();
        }

        static void InitializeLogging()
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

            _log = LogManager.GetLogger("Main");
        }
    }
}
