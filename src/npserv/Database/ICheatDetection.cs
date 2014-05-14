using System;
using BrightstarDB.EntityFramework;

namespace NPSharp.CommandLine.Server.Database
{
    [Entity]
    public interface ICheatDetection
    {
        string Id { get; }

        IUser User { get; }

        uint CheatId { get; set; }

        string Reason { get; set; }

        DateTime ExpiryTime { get; set; }
    }
}