using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NPSharp.Master.Messages
{
    public class MasterServerMessageAttribute : Attribute
    {
        public MasterServerMessageAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; private set; }
    }
}
