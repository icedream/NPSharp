using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NPSharp.Steam;

namespace NPSharp.CommandLine.Server
{
    class DummyAuthenticationHandler : IAuthenticationHandler
    {
        private uint _userID = 0;

        public DummyAuthenticationHandler()
        {
            // TODO: Listener on port 12003 accepting HTTP token retrievals
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
