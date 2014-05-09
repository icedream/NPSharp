using System;
using System.Threading;
using NPSharp.Steam;
using Raven.Abstractions.Data;
using Raven.Client.Connection;
using Raven.Client.Linq;
using Raven.Database;
using Raven.Database.Linq.PrivateExtensions;

namespace NPSharp.Authentication
{
    class DummyAuthenticationHandler : IAuthenticationHandler
    {
        // TODO: RavenDB integration

        private uint _userID;

        private DocumentDatabase _db;

        public DummyAuthenticationHandler(DocumentDatabase db)
        {
            _db = db;
        }

        public AuthenticationResult AuthenticateUser(NPServerClient client, string username, string password)
        {
            return new AuthenticationResult(new CSteamID()
            {
                AccountID = _userID++
            });
        }

        public AuthenticationResult AuthenticateUser(NPServerClient client, string token)
        {
            return new AuthenticationResult(new CSteamID()
            {
                AccountID = _userID++
            });
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
