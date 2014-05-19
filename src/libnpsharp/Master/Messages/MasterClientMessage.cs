using System.Collections.Generic;
using System.Net.Sockets;
using System.Reflection;
using System.Text;

namespace NPSharp.Master.Messages
{
    public abstract class MasterClientMessage
    {
        protected List<string> Properties { get; set; }
 
        internal MasterClientMessage Deserialize(Socket sock)
        {
            // TODO
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

        public virtual byte[] Header { get { return new byte[] {0xFF, 0xFF, 0xFF, 0xFF}; } }

        internal string Name
        {
            get { return GetType().GetCustomAttribute<MasterClientMessageAttribute>().Name; }
        }

        protected abstract void Deserialize(string[] arguments);
    }
}
