using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPSharp.Master.Messages
{
    public class MasterClientMessageAttribute : Attribute
    {
        public MasterClientMessageAttribute(string name)
        {
            Name = name;
        }

        internal string Name { get; private set; }
    }
}
