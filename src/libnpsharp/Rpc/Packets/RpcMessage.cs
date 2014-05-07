using System.Linq;

namespace NPSharp.RPC.Packets
{
    public class RPCMessage
    {
        internal const uint Signature = 0xDEADC0DE; // I wonder if aiw3 changed this since kernal noted it in his source code.

        public uint GetTypeId()
        {
            var packet = (PacketAttribute) GetType().GetCustomAttributes(typeof (PacketAttribute), false).Single();
            return packet.Type;
        }
    }
}
