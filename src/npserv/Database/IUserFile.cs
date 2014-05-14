using BrightstarDB.EntityFramework;

namespace NPSharp.CommandLine.Server.Database
{
    [Entity]
    public interface IUserFile
    {
        string Id { get; }

        IUser User { get; set; }

        string FileName { get; set; }

        byte[] FileData { get; set; }
    }
}