namespace DockerSdk.Containers
{
    /// <summary>
    /// Conditions under which an operation should automatically pull the required image from the appropriate Docker
    /// registry.
    /// </summary>
    public enum PullImageCondition
    {
        /// <summary>
        /// Do not pull the image. If the image does not exist, the operation fails.
        /// </summary>
        Never = 0,

        /// <summary>
        /// Pull the image if it is not already present.
        /// </summary>
        IfMissing,

        /// <summary>
        /// Pull the image, even if it's already present. This is useful to get the newest image for a given tag, for example.
        /// </summary>
        Always,
    }
}
