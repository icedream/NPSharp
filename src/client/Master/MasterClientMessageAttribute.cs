using System;

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
