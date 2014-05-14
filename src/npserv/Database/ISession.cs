using System;
using BrightstarDB.EntityFramework;

namespace NPSharp.CommandLine.Server.Database
{
    [Entity]
    public interface ISession
    {
        string Id { get; }

        IUser User { get; }

        DateTime ExpiryTime { get; set; }
    }
}