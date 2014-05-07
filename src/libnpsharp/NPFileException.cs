using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NPSharp
{
    class NpFileException : Exception
    {
        public NpFileException()
            :base(@"Could not fetch file from NP server.")
        {
        }
    }
}
