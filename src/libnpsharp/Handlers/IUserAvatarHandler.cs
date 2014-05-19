using NPSharp.Steam;

namespace NPSharp.Handlers
{
    /// <summary>
    ///     Represents a handler for all user avatar-related requests.
    /// </summary>
    public interface IUserAvatarHandler
    {
        byte[] GetUserAvatar(CSteamID id);

        byte[] GetDefaultAvatar();
    }
}