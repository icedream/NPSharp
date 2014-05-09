using System;
using NPSharp.CommandLine.Server.Database;
using NPSharp.Steam;
using Raven.Client;

namespace NPSharp.CommandLine.Server
{
    class RavenDatabaseAuthenticationHandler : IAuthenticationHandler
    {
        //private readonly IDocumentStore _database;
        private readonly IDocumentSession _db;

        public RavenDatabaseAuthenticationHandler(IDocumentStore database)
        {
            //_database = database;
            _db = database.OpenSession();
        }

        ~RavenDatabaseAuthenticationHandler()
        {
            _db.Dispose();
        }

        public AuthenticationResult AuthenticateUser(NPServerClient client, string username, string password)
        {
            // Nah, authenticating this way is deprecated as fuck.
            return new AuthenticationResult();
        }

        public AuthenticationResult AuthenticateUser(NPServerClient client, string token)
        {
            //using (var db = _database.OpenSession())
            //{
                var session = _db.Load<Session>(token);
                if (session == null)
                {
                    // Invalid session token
                    return new AuthenticationResult();
                }

                // Remove session now since we don't need it anymore
                _db.Delete(session);
                _db.SaveChanges();

                return new AuthenticationResult(new CSteamID()
                {
                    AccountID = session.User.Id,
                    AccountInstance = 1,
                    AccountType = EAccountType.Individual,
                    AccountUniverse = EUniverse.Public
                });
            //}
        }

        public AuthenticationResult AuthenticateServer(NPServerClient client, string licenseKey)
        {
            throw new NotImplementedException();
        }

        public TicketValidationResult ValidateTicket(NPServerClient client, NPServerClient server)
        {
            throw new NotImplementedException();
        }
    }
}
