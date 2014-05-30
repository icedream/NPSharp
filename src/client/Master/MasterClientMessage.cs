using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using log4net;

namespace NPSharp.Master.Messages
{
    public abstract class MasterClientMessage
    {
        private static readonly ILog Log;
        private byte[] _header = {0xFF, 0xFF, 0xFF, 0xFF};

        static MasterClientMessage()
        {
            Log = LogManager.GetLogger(typeof (MasterClientMessage));
        }

        protected List<string> Properties { get; set; }

        public virtual byte[] Header
        {
            get { return _header; }
            protected set { _header = value; }
        }

        internal string Name
        {
            get { return GetType().GetCustomAttribute<MasterClientMessageAttribute>().Name; }
        }

        internal static MasterClientMessage Deserialize(Socket sock)
        {
            while (sock.Connected && !sock.Poll(2000, SelectMode.SelectRead))
            {
            }

            if (!sock.Connected)
                return null;

            try
            {
                var buffer = new byte[1400]; // max packet size = 1400 bytes in dpmaster
                var length = sock.Receive(buffer);

                if (length == 0)
                {
                    Log.Debug("Received 0 bytes");
                    return null;
                }
                if (length < 4)
                {
                    Log.ErrorFormat("Received incomplete 4-byte header (received {0} bytes instead)", length);
                    throw new ProtocolViolationException("Received incomplete header");
                }

                return Deserialize(buffer.Take(length).ToArray());
            }
            catch (SocketException)
            {
                if (sock.Connected)
                    throw;
                return null;
            }
        }

        internal static MasterClientMessage Deserialize(byte[] buffer)
        {
            var header = buffer.Take(4).ToArray();
            var command = Encoding.ASCII.GetString(buffer, 4, buffer.Length - 4).Trim();
            var commandSplit = command.Split(new[] {'\t', '\r', '\n', '\0', ' '},
                StringSplitOptions.RemoveEmptyEntries);

            var commandName = commandSplit[0];
            var commandArguments = commandSplit.Skip(1).ToArray();

            // Search for a message class which fits to the commandName
            var message =
                (MasterClientMessage) Activator.CreateInstance(Assembly.GetExecutingAssembly()
                    .GetTypes()
                    .Single(
                        t =>
                            t.IsSubclassOf(typeof (MasterClientMessage)) &&
                            t.GetCustomAttribute<MasterClientMessageAttribute>()
                                .Name.Equals(commandName, StringComparison.OrdinalIgnoreCase)));

            // Call the individual deserialize method
            message.Deserialize(commandArguments);
            message.Header = header;

            return message;
        }

        internal byte[] SerializeInternal()
        {
            var buffer = new List<byte>();
            buffer.AddRange(Header);
            buffer.AddRange(Encoding.ASCII.GetBytes(Serialize()));
            buffer.Add(0x0a); // end of command
            return buffer.ToArray();
        }

        protected virtual string Serialize()
        {
            return Name;
        }

        protected abstract void Deserialize(string[] arguments);
    }
}