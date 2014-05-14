using System;
using System.Globalization;
using System.Linq;
using log4net;
using NPSharp.CommandLine.Server.Database;
using NPSharp.Steam;

namespace NPSharp.CommandLine.Server
{
    internal class BrightstarDatabaseAuthenticationHandler : IAuthenticationHandler
    {
        private readonly BrightstarDatabaseContext _db;
        private readonly ILog _log;

        public BrightstarDatabaseAuthenticationHandler(BrightstarDatabaseContext database)
        {
            _log = LogManager.GetLogger("AuthHandler");
            _db = database;
        }

        public AuthenticationResult AuthenticateUser(NPServerClient client, string username, string password)
        {
            // Nah, authenticating this way is deprecated as fuck.
            return new AuthenticationResult();
        }

        public AuthenticationResult AuthenticateUser(NPServerClient client, string token)
        {
            // Check if token is valid
            var resultEnum = _db.Sessions.Where(s => s.Id == token && s.ExpiryTime > DateTime.Now);
            if (!resultEnum.Any())
                return new AuthenticationResult(); // authentication failed because token is invalid

            var session = resultEnum.Single();

            var ar =
                new AuthenticationResult(new CSteamID
                {
                    AccountID = uint.Parse(session.User.Id, NumberStyles.Integer),
                    AccountInstance = 1,
                    AccountType = EAccountType.Individual,
                    AccountUniverse = EUniverse.Public
                });

            _db.DeleteObject(session);
            _db.SaveChanges();
            _log.DebugFormat("Deleted now used session {0}", session.Id);

            return ar;
        }

        public AuthenticationResult AuthenticateServer(NPServerClient client, string licenseKey)
        {
            // TODO: AuthenticateServer
            throw new NotImplementedException();
        }

        public TicketValidationResult ValidateTicket(NPServerClient client, NPServerClient server)
        {
            // TODO: ValidateTicket
            throw new NotImplementedException();
        }

        ~BrightstarDatabaseAuthenticationHandler()
        {
            _db.Dispose();
        }
    }
}