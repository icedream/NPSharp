using NPSharp.NP;

namespace NPSharp.Events
{
    /// <summary>
    ///     A delegate for all general client-related events.
    /// </summary>
    /// <param name="sender">The instance of the NP server</param>
    /// <param name="args">All related arguments to this event</param>
    public delegate void ClientEventHandler(NPServer sender, ClientEventArgs args);
}