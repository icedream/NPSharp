using BrightstarDB.EntityFramework;

namespace NPSharp.CommandLine.Server.Database
{
    [Entity]
    public interface IFriend
    {
        string Id { get; }

        IUser User { get; }

        uint FriendUserId { get; set; }

        string FriendName { get; set; }
    }
}