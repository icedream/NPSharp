using System;
using BrightstarDB.EntityFramework;

namespace NPSharp.CommandLine.Server.Database
{
    [Entity]
    public interface ISession
    {
        [Identifier]
        string Id { get; }

        IUser User { get; }

        DateTime ExpiryTime { get; set; }
    }
}