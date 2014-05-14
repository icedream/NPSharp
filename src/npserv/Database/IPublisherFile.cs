using BrightstarDB.EntityFramework;

namespace NPSharp.CommandLine.Server.Database
{
    [Entity]
    public interface IPublisherFile
    {
        string Id { get; }

        string FileName { get; set; }

        byte[] FileData { get; set; }
    }
}