using System.Net;
using System.Net.NetworkInformation;
using DockerSdk.Containers;

namespace DockerSdk.Networks
{
    /// <summary>
    /// Provides information about a Docker network endpoint. An endpoint is a connection between a Docker container and
    /// a Docker network.
    /// </summary>
    /// <remarks>This class holds a snapshot in time. Its information is immutable once created.</remarks>
    public interface INetworkEndpoint
    {
        /// <summary>
        /// Gets the Docker container that this endpoint connects to.
        /// </summary>
        public IContainer Container { get; }

        /// <summary>
        /// Gets the endpoint's ID.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Gets the container's IPv4 address (if it has one) on this endpoint's network.
        /// </summary>
        public IPAddress? IPv4Address { get; }

        /// <summary>
        /// Gets the container's IPv6 address (if it has one) on this endpoint's network.
        /// </summary>
        public IPAddress? IPv6Address { get; }

        /// <summary>
        /// Gets the container's MAC address on this endpoint's network.
        /// </summary>
        public PhysicalAddress MacAddress { get; }

        /// <summary>
        /// Gets the Docker network that this endpoint connects to.
        /// </summary>
        public INetwork Network { get; }

        // Depending on which Docker API we called, we also received gateway and/or subnet information. We don't include
        // those because, firstly, they're information about the network rather than of the endpoint itself, and
        // secondly, because it's bad form to have fields that may or may not be populated depending on how the object
        // was created.
    }
}
