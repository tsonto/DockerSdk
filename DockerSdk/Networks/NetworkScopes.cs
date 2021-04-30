namespace DockerSdk.Networks
{
    /// <summary>
    /// An indication of which abstraction level a network exists at.
    /// </summary>
    public enum NetworkScope
    {
        /// <summary>
        /// Networks with local scope can exist only on a single host, and are wholly managed by that host's Docker daemon.
        /// </summary>
        /// <remarks>Some network drivers, such as the "overlay" driver, do not support local scope.</remarks>
        Local = 1,

        /// <summary>
        /// Networks with swarm scope (also called global scope) can span multiple Docker hosts.
        /// </summary>
        /// <remarks>Some network drivers, such as the "none" driver, do not support swarm scope.</remarks>
        Swarm = 2,
    }
}
