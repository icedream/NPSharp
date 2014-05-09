using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPSharp.CommandLine.Server
{
    class DummyFileServingHandler : IFileServingHandler
    {
        public byte[] ReadUserFile(NPServerClient client, string file)
        {
            return new byte[0];
        }

        public byte[] ReadPublisherFile(NPServerClient client, string file)
        {
            switch (file.ToLower())
            {
                case "hello_world.txt":
                    return Encoding.UTF8.GetBytes("Hi, this is a test hello_world.txt.");
                case "motd-english.txt":
                    return
                        Encoding.UTF8.GetBytes(
                            "Hello, this is a test NP server written in C#. Thanks for visiting this server.");
            }
            return null;
        }

        public void WriteUserFile(NPServerClient client, string file, byte[] data)
        {
            // Ignore stuff
        }
    }
}
