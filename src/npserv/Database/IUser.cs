using System;
using System.Collections.Generic;
using BrightstarDB.EntityFramework;

namespace NPSharp.CommandLine.Server.Database
{
    [Entity]
    public interface IUser
    {
        string Id { get; }

        string UserName { get; set; }

        string UserMail { get; set; }

        string PasswordHash { get; set; }

        DateTime LastLogin { get; set; }

        [InverseProperty("User")]
        ICollection<ISession> Sessions { get; set; }

        [InverseProperty("User")]
        ICollection<IBan> Bans { get; set; }

        [InverseProperty("User")]
        ICollection<ICheatDetection> CheatDetections { get; set; }

        [InverseProperty("User")]
        ICollection<IUserFile> UserFiles { get; set; }

        [InverseProperty("User")]
        ICollection<IFriend> FriendIDs { get; set; }
    }
}