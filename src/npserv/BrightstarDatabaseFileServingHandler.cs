using System.Globalization;
using System.Linq;
using System.Text;
using NPSharp.CommandLine.Server.Database;
using NPSharp.Handlers;
using NPSharp.NP;

namespace NPSharp.CommandLine.Server
{
    internal class BrightstarDatabaseFileServingHandler : IFileServingHandler
    {
        private readonly BrightstarDatabaseContext _db;

        public BrightstarDatabaseFileServingHandler(BrightstarDatabaseContext database)
        {
            //_database = database;
            _db = database;
        }

        public byte[] ReadUserFile(NPServerClient client, string file)
        {
            var resultEnum =
                _db.UserFiles.Where(
                    uf =>
                        uf.User.Id == client.UserID.AccountID.ToString(CultureInfo.InvariantCulture) &&
                        uf.FileName == file);

            return resultEnum.Any() ? resultEnum.Single().FileData : GetDefaultUserFile(file);
        }

        public byte[] ReadPublisherFile(NPServerClient client, string file)
        {
            var resultEnum =
                _db.PublisherFiles.Where(pf => pf.FileName == file).ToArray();

            return resultEnum.Any() ? resultEnum.Single().FileData : GetDefaultPublisherFile(file);
        }

        public void WriteUserFile(NPServerClient client, string file, byte[] data)
        {
            var resultEnum =
                _db.UserFiles.Where(
                    uf =>
                        uf.User.Id == client.UserID.AccountID.ToString(CultureInfo.InvariantCulture) &&
                        uf.FileName == file)
                        .ToArray();

            var userFile = resultEnum.Any() ? resultEnum.Single() : _db.UserFiles.Create();
            userFile.FileName = file;
            userFile.FileData = data;
            userFile.User = _db.Users.Single(u => u.Id == client.UserID.AccountID.ToString(CultureInfo.InvariantCulture));

            _db.SaveChanges();
        }

        ~BrightstarDatabaseFileServingHandler()
        {
            _db.Dispose();
        }

        protected byte[] GetDefaultUserFile(string file)
        {
            switch (file)
            {
                case "iw4.stat":
                    return new byte[8*1024];
                default:
                    return null;
            }
        }

        protected byte[] GetDefaultPublisherFile(string file)
        {
            switch (file)
            {
                case "hello_world.txt":
                case "motd-english.txt":
                case "motd-german.txt":
                case "motd-french.txt":
                case "motd-russian.txt":
                case "motd-spanish.txt":
                    return Encoding.UTF8.GetBytes("hello");
                case "playerlog.csv":
                case "social_tu1.cfg":
                case "heatmap.raw":
                case "online_mp.img":
                    return new byte[0];
                default:
                    return null;
            }
        }
    }
}