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
    internal class NetworkEndpoint : INetworkEndpoint
    {
        public NetworkEndpoint(string id, INetwork network, IContainer container)
        {
            Id = id;
            Network = network;
            Container = container;
        }

        /// <inheritdoc/>
        public IContainer Container { get; }

        /// <inheritdoc/>
        public string Id { get; }

        /// <inheritdoc/>
        public IPAddress? IPv4Address { get; init; }

        /// <inheritdoc/>
        public IPAddress? IPv6Address { get; init; }

        /// <inheritdoc/>
        public PhysicalAddress MacAddress { get; init; } = PhysicalAddress.None;

        /// <inheritdoc/>
        public INetwork Network { get; }
    }
}
