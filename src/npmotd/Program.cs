using System;
using System.Text;
using NPSharp.Authentication;

namespace NPSharp.CommandLine.MOTD
{
    class Program
    {
        static void Main(string[] args)
        {
            var hostname = args[0];
            var username = args[1];
            var password = args[2];

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

            var np = new NPClient(hostname);
            try
            {
                np.AuthenticateWithToken(ah.SessionToken).Wait();
            }
            catch
            {
                Console.Error.WriteLine("Authenticated but session token was invalid.");
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
