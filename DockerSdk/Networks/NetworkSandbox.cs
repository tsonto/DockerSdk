namespace DockerSdk.Networks
{
    /// <summary>
    /// Represents a network sandbox within a Docker container. A sandbox is in charge of the container's network
    /// endpoints.
    /// </summary>
    public record NetworkSandbox
    {
        /// <summary>
        /// Gets the ID of the sandbox.
        /// </summary>
        /// <remarks>
        /// This library's authors don't have any guesses about what this may be useful for. It's just an odd bit of
        /// trivia returned by the Docker REST API.
        /// </remarks>
        public string SandboxId { get; }

        /// <summary>
        /// Gets the key string of the sandbox.
        /// </summary>
        /// <remarks>
        /// This library's authors don't know what this is or what it's for. It's just an odd bit of trivia returned by
        /// the Docker REST API.
        /// </remarks>
        public string SandboxKey { get; }

        /// <summary>
        /// Creates a new instances of the <see cref="NetworkSandbox"/> class.
        /// </summary>
        /// <param name="sandboxID">The sandbox's ID.</param>
        /// <param name="sandboxKey">The sandbox's key.</param>
        internal NetworkSandbox(string sandboxID, string sandboxKey)
        {
            SandboxId = sandboxID;
            SandboxKey = sandboxKey;
        }
    }
}
