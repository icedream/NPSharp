using System;

namespace NPSharp.NP
{
    internal class NpFileException : Exception
    {
        internal NpFileException(int error)
            : base(error == 1 ? @"File not found on NP server" : @"Internal error on NP server")
        {
        }

        internal NpFileException()
            : base(@"Could not fetch file from NP server.")
        {
        }
    }
}