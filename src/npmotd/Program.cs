using System;
using System.Text;
using NPSharp.Authentication;

namespace NPSharp.CommandLine.MOTD
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 4)
            {
                Console.Error.WriteLine("Needs 4 arguments: hostname port username password");
                return;
            }

            var hostname = args[0];
            var port = ushort.Parse(args[1]);
            var username = args[2];
            var password = args[3];

            // NP connection setup
            var np = new NPClient(hostname, port);
            if (!np.Connect())
            {
                Console.Error.WriteLine("Connection to NP server failed.");
                return;
            }

            // Get session token
            var ah = new AuthenticationHelper(hostname);
            try
            {
                ah.Authenticate(username, password);
            }
            catch (Exception err)
            {
#if DEBUG
                Console.Error.WriteLine("Could not authenticate: {0}", err);
#else
                Console.Error.WriteLine("Could not authenticate: {0}", err.Message);
#endif
                return;
            }

            // Validate authentication using session token
            try
            {
                np.AuthenticateWithToken(ah.SessionToken).Wait();
            }
            catch (Exception err)
            {
#if DEBUG
                Console.Error.WriteLine("Authenticated but session token was invalid. {0}", err);
#else
                Console.Error.WriteLine("Authenticated but session token was invalid ({0}).", err.Message);
#endif
                return;
            }

            try
            {
                Console.WriteLine(Encoding.UTF8.GetString(np.GetPublisherFile("motd-english.txt").Result));
            }
            catch
            {
                Console.Error.WriteLine("Could not read MOTD from NP server.");
            }



        }
    }
}
