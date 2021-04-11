namespace DockerSdk.Events
{
    /// <summary>
    /// Represents the kinds of resource or other entity that events can be about.
    /// </summary>
    public enum EventSubjectType
    {
        /// <summary>
        /// A Docker container.
        /// </summary>
        Container,

        /// <summary>
        /// A Docker image.
        /// </summary>
        Image,

        /// <summary>
        /// A Docker volume.
        /// </summary>
        Volume,

        /// <summary>
        /// A Docker network.
        /// </summary>
        Network,

        /// <summary>
        /// The Docker daemon.
        /// </summary>
        Daemon,

        /// <summary>
        /// A Docker swarm service.
        /// </summary>
        Service,

        /// <summary>
        /// A Docker swarm node.
        /// </summary>
        Node,

        /// <summary>
        /// A Docker swarm secret.
        /// </summary>
        Secret,

        /// <summary>
        /// A Docker swarm config.
        /// </summary>
        Config,
    }
}
