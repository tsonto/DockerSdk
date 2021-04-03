namespace DockerSdk.Containers
{
    /// <summary>
    /// One of the discrete states that a Docker image can be in.
    /// </summary>
    public enum ContainerStatus
    {
        /// <summary>
        /// The container exists but has not been started.
        /// </summary>
        Created,

        /// <summary>
        /// The container is resetting to run again.
        /// </summary>
        Restarting,

        /// <summary>
        /// The container has live processes.
        /// </summary>
        Running,

        /// <summary>
        /// The container is being removed. At this point it is not running.
        /// </summary>
        Removing,

        /// <summary>
        /// The container is paused. Its processes are not running, but can be resumed.
        /// </summary>
        Paused,

        /// <summary>
        /// The container stopped gracefully and is being removed. This status can apply regardless of exit code.
        /// </summary>
        Exited,

        /// <summary>
        /// The daemon shut down the container non-gracefully.
        /// </summary>
        Dead,
    }
}
