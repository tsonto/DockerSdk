namespace DockerSdk.Containers
{
    /// <summary>
    /// Lists actions that Docker can do automatically when a container exits.
    /// </summary>
    public enum ContainerExitAction
    {
        /// <summary>
        /// Take no special action when the container exits.
        /// </summary>
        None = 0,

        /// <summary>
        /// Automatically remove the container when it exits.
        /// </summary>
        Remove,

        /// <summary>
        /// Automatically restart the container whenever it exits.
        /// </summary>
        Restart,

        /// <summary>
        /// Automatically restart the container when it exits, unless it was specifically stopped.
        /// </summary>
        RestartUnlessStopped,

        /// <summary>
        /// Automatically restart the container when it exits, but only if the exit code is non-zero.
        /// </summary>
        /// <seealso cref="CreateContainerOptions.MaximumRetriesCount"/>
        RestartOnFailure,
    }
}
