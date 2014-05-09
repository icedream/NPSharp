using System.Linq;
using System.Text;
using NPSharp.CommandLine.Server.Database;
using Raven.Client;

namespace NPSharp.CommandLine.Server
{
    class RavenDatabaseFileServingHandler : IFileServingHandler
    {
        private readonly IDocumentStore _database;

        public RavenDatabaseFileServingHandler(IDocumentStore database)
        {
            _database = database;
        }

        protected byte[] GetDefaultUserFile(string file)
        {
            switch (file)
            {
                case "iw4.stat":
                    return new byte[8 * 1024];
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

        public byte[] ReadUserFile(NPServerClient client, string file)
        {
            using (var db = _database.OpenSession())
            {
                var userfile = db
                    .Query<UserFile>()
                    .Customize(uf => uf.Include<UserFile>(o => o.UserID))
                    .SingleOrDefault(uf =>
                        uf.FileName == file
                        && db.Load<User>(uf.UserID).Id == client.UserID.AccountID);
                
                return userfile == null ? null : userfile.FileData;
            }
        }

        public byte[] ReadPublisherFile(NPServerClient client, string file)
        {
            using (var db = _database.OpenSession())
            {
                var pubfiles =
                    db.Query<PublisherFile>()
                        .Where(uf => uf.FileName == file)
                        .ToArray();

                return !pubfiles.Any() ? GetDefaultPublisherFile(file) : pubfiles.Single().FileData;
            }
        }

        public void WriteUserFile(NPServerClient client, string file, byte[] data)
        {
            using (var db = _database.OpenSession())
            {
                var userfile = db
                    .Query<UserFile>()
                    .Customize(uf => uf.Include<UserFile>(o => o.UserID))
                    .SingleOrDefault(uf =>
                        uf.FileName == file
                        && db.Load<User>(uf.UserID).Id == client.UserID.AccountID)
                     ?? new UserFile();

                userfile.UserID = client.UserID.AccountID;
                userfile.FileData = data;
                userfile.FileName = file;

                db.Store(userfile);
                db.SaveChanges();
            }
        }
    }
}
