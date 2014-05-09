using System;

namespace NPSharp
{
    public class ClientEventArgs : EventArgs
    {
        internal ClientEventArgs(NPServerClient client)
        {
            Client = client;
        }

        public NPServerClient Client { get; private set; }
    }
}