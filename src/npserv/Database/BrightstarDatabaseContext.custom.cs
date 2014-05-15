using System;
using System.Linq;

namespace NPSharp.CommandLine.Server.Database
{
    public partial class BrightstarDatabaseContext
    {
        public IUser CreateUser(string name, string email, string password)
        {
            if (UserExists(name))
                throw new DatabaseUserExistsException();

            var user = Users.Create();
            user.UserName = name;
            user.UserMail = email;
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
            user.UserNumber = _genUserNumber(/*user.Id*/);

            return user;
        }

        private uint _genUserNumber(/*string userId*/)
        {
            /*
            // for some reason we sometimes get full URIs here.
            userId = userId.Split('/').Last().Replace("-", "");

            // Since the string is a hexified UNIQUE identifier,
            // use the numeric representation of it.
            var userNum = uint.Parse(userId, NumberStyles.HexNumber);
            */

            // The above doesn't work since the GUID has a few bits too much :P
            // So instead - even though taking more queries - we will use the user
            // count to approximate a new user ID.
            var userNum = (uint)Users.Count() + 1;
            while (Users.Count(u => u.UserNumber == userNum) > 0)
                userNum++;

            return userNum;
        }

        public bool UserExists(string userName)
        {
            return GetUser(userName) != null;
        }

        public IUser GetUser(string userName)
        {
            var users = Users.Where(u => u.UserName == userName).ToArray();
            return users.Any() ? users.Single() : null;
        }

        /// <summary>
        /// Creates a user session.
        /// </summary>
        /// <param name="user">The user to assign the session to.</param>
        /// <param name="validTimeSpan">The time span in seconds. Default: 3 minutes.</param>
        /// <returns>The newly created user session</returns>
        public ISession CreateSession(IUser user, uint validTimeSpan = 3 * 60)
        {
            var session = Sessions.Create();
            session.ExpiryTime = DateTime.Now + TimeSpan.FromSeconds(validTimeSpan);

            user.Sessions.Add(session);

            return session;
        }

        /// <summary>
        /// Tries to find the wanted session and drops it if it's valid,
        /// therefore "using it".
        /// </summary>
        /// <param name="sessionToken">The token of the wanted session</param>
        /// <param name="callback">The callback to use for session results (goes for both invalid and valid sessions)</param>
        /// <returns>The found session if the session is validated successfully, otherwise null.</returns>
        public void ValidateSession(string sessionToken, Action<ISession> callback)
        {
            var sessions = Sessions
                .Where(s => s.Id == sessionToken).ToArray() // database level query
                .Where(s => s.ExpiryTime > DateTime.Now).ToArray(); // local level query (seems like this isn't supported [yet])

            // We have to use a callback here since deleting the object from database
            // will also release it from .NET's management and therefore makes the object
            // invalid.
            if (!sessions.Any())
                callback(null);
            else
            {
                var session = sessions.Single();
                callback(session);
                DeleteObject(session);
            }
        }
    }
}
