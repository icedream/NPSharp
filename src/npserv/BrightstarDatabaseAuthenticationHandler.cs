using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using log4net;
using NPSharp.CommandLine.Server.Database;
using NPSharp.Handlers;
using NPSharp.NP;
using NPSharp.RPC.Messages.Data;
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

        public NPAuthenticationResult AuthenticateUser(NPServerClient client, string username, string password)
        {
            // Nah, authenticating this way is deprecated as fuck.
            return new NPAuthenticationResult();
        }

        public NPAuthenticationResult AuthenticateUser(NPServerClient client, string token)
        {
            var ar = new NPAuthenticationResult();

            // Check if token is valid
            _db.ValidateSession(token, session =>
            {
                if (session == null)
                {
                    return;
                }

                ar =
                    new NPAuthenticationResult(new CSteamID
                    {
                        AccountID = session.User.UserNumber,
                        AccountInstance = 1,
                        AccountType = EAccountType.Individual,
                        AccountUniverse = EUniverse.Public
                    });

                _log.DebugFormat("Deleting validated session {0}", session.Id);
            });
            _db.SaveChanges();

            return ar;
        }

        public NPAuthenticationResult AuthenticateServer(NPServerClient client, string licenseKey)
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