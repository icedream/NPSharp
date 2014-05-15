using System;
using System.Collections.Generic;
using BrightstarDB.Client;
using BrightstarDB.EntityFramework;

namespace NPSharp.CommandLine.Server.Database
{
    public partial class BrightstarDatabaseContext : BrightstarEntityContext
    {
        private static readonly EntityMappingStore TypeMappings;

        static BrightstarDatabaseContext()
        {
            TypeMappings = new EntityMappingStore();
            var provider = new ReflectionMappingProvider();
            provider.AddMappingsForType(TypeMappings, typeof (IBan));
            TypeMappings.SetImplMapping<IBan, Ban>();
            provider.AddMappingsForType(TypeMappings, typeof (ICheatDetection));
            TypeMappings.SetImplMapping<ICheatDetection, CheatDetection>();
            provider.AddMappingsForType(TypeMappings, typeof (IFriend));
            TypeMappings.SetImplMapping<IFriend, Friend>();
            provider.AddMappingsForType(TypeMappings, typeof (IPublisherFile));
            TypeMappings.SetImplMapping<IPublisherFile, PublisherFile>();
            provider.AddMappingsForType(TypeMappings, typeof (ISession));
            TypeMappings.SetImplMapping<ISession, Session>();
            provider.AddMappingsForType(TypeMappings, typeof (IUser));
            TypeMappings.SetImplMapping<IUser, User>();
            provider.AddMappingsForType(TypeMappings, typeof (IUserFile));
            TypeMappings.SetImplMapping<IUserFile, UserFile>();
        }

        /// <summary>
        ///     Initialize a new entity context using the specified BrightstarDB
        ///     Data Object Store connection
        /// </summary>
        /// <param name="dataObjectStore">The connection to the BrightstarDB Data Object Store that will provide the entity objects</param>
        /// <param name="typeMappings">
        ///     OPTIONAL: A <see cref="EntityMappingStore" /> that overrides the default mappings generated
        ///     by reflection.
        /// </param>
        public BrightstarDatabaseContext(IDataObjectStore dataObjectStore, EntityMappingStore typeMappings = null)
            : base(typeMappings ?? TypeMappings, dataObjectStore)
        {
            InitializeContext();
        }

        /// <summary>
        ///     Initialize a new entity context using the specified Brightstar connection string
        /// </summary>
        /// <param name="connectionString">The connection to be used to connect to an existing BrightstarDB store</param>
        /// <param name="enableOptimisticLocking">OPTIONAL: If set to true optmistic locking will be applied to all entity updates</param>
        /// <param name="updateGraphUri">
        ///     OPTIONAL: The URI identifier of the graph to be updated with any new triples created by operations on the store. If
        ///     not defined, the default graph in the store will be updated.
        /// </param>
        /// <param name="datasetGraphUris">
        ///     OPTIONAL: The URI identifiers of the graphs that will be queried to retrieve entities and their properties.
        ///     If not defined, all graphs in the store will be queried.
        /// </param>
        /// <param name="versionGraphUri">
        ///     OPTIONAL: The URI identifier of the graph that contains version number statements for entities.
        ///     If not defined, the <paramref name="updateGraphUri" /> will be used.
        /// </param>
        /// <param name="typeMappings">
        ///     OPTIONAL: A <see cref="EntityMappingStore" /> that overrides the default mappings generated
        ///     by reflection.
        /// </param>
        public BrightstarDatabaseContext(
            string connectionString,
            bool? enableOptimisticLocking = null,
            string updateGraphUri = null,
            IEnumerable<string> datasetGraphUris = null,
            string versionGraphUri = null,
            EntityMappingStore typeMappings = null
            )
            : base(
                typeMappings ?? TypeMappings, connectionString, enableOptimisticLocking, updateGraphUri,
                datasetGraphUris, versionGraphUri)
        {
            InitializeContext();
        }

        /// <summary>
        ///     Initialize a new entity context using the specified Brightstar
        ///     connection string retrieved from the configuration.
        /// </summary>
        /// <param name="typeMappings">
        ///     OPTIONAL: A <see cref="EntityMappingStore" /> that overrides the default mappings generated
        ///     by reflection.
        /// </param>
        public BrightstarDatabaseContext(EntityMappingStore typeMappings = null) : base(typeMappings ?? TypeMappings)
        {
            InitializeContext();
        }

        //  specified target graphs
        /// <summary>
        ///     Initialize a new entity context using the specified Brightstar
        ///     connection string retrieved from the configuration and the
        /// </summary>
        /// <param name="updateGraphUri">
        ///     The URI identifier of the graph to be updated with any new triples created by operations on the store. If
        ///     set to null, the default graph in the store will be updated.
        /// </param>
        /// <param name="datasetGraphUris">
        ///     The URI identifiers of the graphs that will be queried to retrieve entities and their properties.
        ///     If set to null, all graphs in the store will be queried.
        /// </param>
        /// <param name="versionGraphUri">
        ///     The URI identifier of the graph that contains version number statements for entities.
        ///     If set to null, the value of <paramref name="updateGraphUri" /> will be used.
        /// </param>
        /// <param name="typeMappings">
        ///     OPTIONAL: A <see cref="EntityMappingStore" /> that overrides the default mappings generated
        ///     by reflection.
        /// </param>
        public BrightstarDatabaseContext(
            string updateGraphUri,
            IEnumerable<string> datasetGraphUris,
            string versionGraphUri,
            EntityMappingStore typeMappings = null
            ) : base(typeMappings ?? TypeMappings, updateGraphUri, datasetGraphUris, versionGraphUri)
        {
            InitializeContext();
        }

        public IEntitySet<IBan> Bans { get; private set; }

        public IEntitySet<ICheatDetection> CheatDetections { get; private set; }

        public IEntitySet<IFriend> Friends { get; private set; }

        public IEntitySet<IPublisherFile> PublisherFiles { get; private set; }

        public IEntitySet<ISession> Sessions { get; private set; }

        public IEntitySet<IUser> Users { get; private set; }

        public IEntitySet<IUserFile> UserFiles { get; private set; }

        private void InitializeContext()
        {
            Bans = new BrightstarEntitySet<IBan>(this);
            CheatDetections = new BrightstarEntitySet<ICheatDetection>(this);
            Friends = new BrightstarEntitySet<IFriend>(this);
            PublisherFiles = new BrightstarEntitySet<IPublisherFile>(this);
            Sessions = new BrightstarEntitySet<ISession>(this);
            Users = new BrightstarEntitySet<IUser>(this);
            UserFiles = new BrightstarEntitySet<IUserFile>(this);
        }
    }
}

namespace NPSharp.CommandLine.Server.Database
{
    public class Ban : BrightstarEntityObject, IBan
    {
        public Ban(BrightstarEntityContext context, IDataObject dataObject) : base(context, dataObject)
        {
        }

        public Ban()
        {
        }

        public String Id
        {
            get { return GetIdentity(); }
            set { SetIdentity(value); }
        }

        #region Implementation of NPSharp.CommandLine.Server.Database.IBan

        public IUser User
        {
            get { return GetRelatedObject<IUser>("User"); }
        }

        public String Reason
        {
            get { return GetRelatedProperty<String>("Reason"); }
            set { SetRelatedProperty("Reason", value); }
        }

        public DateTime ExpiryTime
        {
            get { return GetRelatedProperty<DateTime>("ExpiryTime"); }
            set { SetRelatedProperty("ExpiryTime", value); }
        }

        #endregion
    }
}

namespace NPSharp.CommandLine.Server.Database
{
    public class CheatDetection : BrightstarEntityObject, ICheatDetection
    {
        public CheatDetection(BrightstarEntityContext context, IDataObject dataObject) : base(context, dataObject)
        {
        }

        public CheatDetection()
        {
        }

        public String Id
        {
            get { return GetIdentity(); }
            set { SetIdentity(value); }
        }

        #region Implementation of NPSharp.CommandLine.Server.Database.ICheatDetection

        public IUser User
        {
            get { return GetRelatedObject<IUser>("User"); }
        }

        public UInt32 CheatId
        {
            get { return GetRelatedProperty<UInt32>("CheatId"); }
            set { SetRelatedProperty("CheatId", value); }
        }

        public String Reason
        {
            get { return GetRelatedProperty<String>("Reason"); }
            set { SetRelatedProperty("Reason", value); }
        }

        public DateTime ExpiryTime
        {
            get { return GetRelatedProperty<DateTime>("ExpiryTime"); }
            set { SetRelatedProperty("ExpiryTime", value); }
        }

        #endregion
    }
}

namespace NPSharp.CommandLine.Server.Database
{
    public class Friend : BrightstarEntityObject, IFriend
    {
        public Friend(BrightstarEntityContext context, IDataObject dataObject) : base(context, dataObject)
        {
        }

        public Friend()
        {
        }

        public String Id
        {
            get { return GetIdentity(); }
            set { SetIdentity(value); }
        }

        #region Implementation of NPSharp.CommandLine.Server.Database.IFriend

        public IUser User
        {
            get { return GetRelatedObject<IUser>("User"); }
        }

        public UInt32 FriendUserId
        {
            get { return GetRelatedProperty<UInt32>("FriendUserId"); }
            set { SetRelatedProperty("FriendUserId", value); }
        }

        public String FriendName
        {
            get { return GetRelatedProperty<String>("FriendName"); }
            set { SetRelatedProperty("FriendName", value); }
        }

        #endregion
    }
}

namespace NPSharp.CommandLine.Server.Database
{
    public class PublisherFile : BrightstarEntityObject, IPublisherFile
    {
        public PublisherFile(BrightstarEntityContext context, IDataObject dataObject) : base(context, dataObject)
        {
        }

        public PublisherFile()
        {
        }

        public String Id
        {
            get { return GetIdentity(); }
            set { SetIdentity(value); }
        }

        #region Implementation of NPSharp.CommandLine.Server.Database.IPublisherFile

        public String FileName
        {
            get { return GetRelatedProperty<String>("FileName"); }
            set { SetRelatedProperty("FileName", value); }
        }

        public Byte[] FileData
        {
            get { return GetRelatedProperty<Byte[]>("FileData"); }
            set { SetRelatedProperty("FileData", value); }
        }

        #endregion
    }
}

namespace NPSharp.CommandLine.Server.Database
{
    public class Session : BrightstarEntityObject, ISession
    {
        public Session(BrightstarEntityContext context, IDataObject dataObject) : base(context, dataObject)
        {
        }

        public Session()
        {
        }

        public String Id
        {
            get { return GetIdentity(); }
            set { SetIdentity(value); }
        }

        #region Implementation of NPSharp.CommandLine.Server.Database.ISession

        public IUser User
        {
            get { return GetRelatedObject<IUser>("User"); }
        }

        public DateTime ExpiryTime
        {
            get { return GetRelatedProperty<DateTime>("ExpiryTime"); }
            set { SetRelatedProperty("ExpiryTime", value); }
        }

        #endregion
    }
}

namespace NPSharp.CommandLine.Server.Database
{
    public class User : BrightstarEntityObject, IUser
    {
        public User(BrightstarEntityContext context, IDataObject dataObject) : base(context, dataObject)
        {
        }

        public User()
        {
        }

        public String Id
        {
            get { return GetIdentity(); }
            set { SetIdentity(value); }
        }

        #region Implementation of NPSharp.CommandLine.Server.Database.IUser

        public String UserName
        {
            get { return GetRelatedProperty<String>("UserName"); }
            set { SetRelatedProperty("UserName", value); }
        }

        public String UserMail
        {
            get { return GetRelatedProperty<String>("UserMail"); }
            set { SetRelatedProperty("UserMail", value); }
        }

        public UInt32 UserNumber
        {
            get { return GetRelatedProperty<UInt32>("UserNumber"); }
            set { SetRelatedProperty("UserNumber", value); }
        }

        public String PasswordHash
        {
            get { return GetRelatedProperty<String>("PasswordHash"); }
            set { SetRelatedProperty("PasswordHash", value); }
        }

        public DateTime LastLogin
        {
            get { return GetRelatedProperty<DateTime>("LastLogin"); }
            set { SetRelatedProperty("LastLogin", value); }
        }

        public ICollection<ISession> Sessions
        {
            get { return GetRelatedObjects<ISession>("Sessions"); }
            set
            {
                if (value == null) throw new ArgumentNullException("value");
                SetRelatedObjects("Sessions", value);
            }
        }

        public ICollection<IBan> Bans
        {
            get { return GetRelatedObjects<IBan>("Bans"); }
            set
            {
                if (value == null) throw new ArgumentNullException("value");
                SetRelatedObjects("Bans", value);
            }
        }

        public ICollection<ICheatDetection> CheatDetections
        {
            get { return GetRelatedObjects<ICheatDetection>("CheatDetections"); }
            set
            {
                if (value == null) throw new ArgumentNullException("value");
                SetRelatedObjects("CheatDetections", value);
            }
        }

        public ICollection<IUserFile> UserFiles
        {
            get { return GetRelatedObjects<IUserFile>("UserFiles"); }
            set
            {
                if (value == null) throw new ArgumentNullException("value");
                SetRelatedObjects("UserFiles", value);
            }
        }

        public ICollection<IFriend> FriendIDs
        {
            get { return GetRelatedObjects<IFriend>("FriendIDs"); }
            set
            {
                if (value == null) throw new ArgumentNullException("value");
                SetRelatedObjects("FriendIDs", value);
            }
        }

        #endregion
    }
}

namespace NPSharp.CommandLine.Server.Database
{
    public class UserFile : BrightstarEntityObject, IUserFile
    {
        public UserFile(BrightstarEntityContext context, IDataObject dataObject) : base(context, dataObject)
        {
        }

        public UserFile()
        {
        }

        public String Id
        {
            get { return GetIdentity(); }
            set { SetIdentity(value); }
        }

        #region Implementation of NPSharp.CommandLine.Server.Database.IUserFile

        public IUser User
        {
            get { return GetRelatedObject<IUser>("User"); }
            set { SetRelatedObject("User", value); }
        }

        public String FileName
        {
            get { return GetRelatedProperty<String>("FileName"); }
            set { SetRelatedProperty("FileName", value); }
        }

        public Byte[] FileData
        {
            get { return GetRelatedProperty<Byte[]>("FileData"); }
            set { SetRelatedProperty("FileData", value); }
        }

        #endregion
    }
}