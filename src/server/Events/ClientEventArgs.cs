using System;
using NPSharp.NP;

namespace NPSharp.Events
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