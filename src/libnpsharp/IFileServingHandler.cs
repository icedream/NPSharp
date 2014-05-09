namespace NPSharp
{
    /// <summary>
    ///     Represents a handler for all file-related requests.
    /// </summary>
    public interface IFileServingHandler
    {
        /// <summary>
        ///     Reads a file assigned to the user.
        /// </summary>
        /// <returns>The file contents as byte array or null if the file could not be read, opened or generated</returns>
        /// <param name="client">NP server client of the user</param>
        /// <param name="file">The file name</param>
        byte[] ReadUserFile(NPServerClient client, string file);

        /// <summary>
        ///     Reads a publisher file.
        /// </summary>
        /// <returns>The file contents as byte array or null if the file could not be read, opened or generated</returns>
        /// <param name="client">NP server client of the user</param>
        /// <param name="file">The file name</param>
        byte[] ReadPublisherFile(NPServerClient client, string file);

        /// <summary>
        ///     Writes a file and assigns it to the client user.
        /// </summary>
        /// <param name="client">NP server client of the user</param>
        /// <param name="file">The file name</param>
        /// <param name="data">The file contents as byte array</param>
        void WriteUserFile(NPServerClient client, string file, byte[] data);
    }
}