using System;
using BrightstarDB.EntityFramework;

namespace NPSharp.CommandLine.Server.Database
{
    [Entity]
    public interface IBan
    {
        string Id { get; }

        IUser User { get; }

        string Reason { get; set; }

        DateTime ExpiryTime { get; set; }
    }
}