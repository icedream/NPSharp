using System;

namespace NPSharp
{
    public class ClientEventArgs : EventArgs
    {
        internal ClientEventArgs(NPServer.NPServerClient client)
        {
            Client = client;
        }

        public NPServer.NPServerClient Client { get; private set; }
    }
}
