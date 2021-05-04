using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using DockerSdk.Containers;

namespace DockerSdk.Networks
{
    /// <summary>
    /// Provides information about a Docker network endpoint. An endpoint is a connection between a Docker container and
    /// a Docker network.
    /// </summary>
    /// <remarks>This class holds a snapshot in time. Its information is immutable once created.</remarks>
    public class NetworkEndpoint: INetworkEndpoint
    {
        /// <inheritdoc/>
        public IContainer Container { get; internal set; }

        /// <inheritdoc/>
        public string Id { get; init; }

        /// <inheritdoc/>
        public IPAddress? IPv4Address { get; init; }

        /// <inheritdoc/>
        public IPAddress? IPv6Address { get; init; }

        /// <inheritdoc/>
        public PhysicalAddress MacAddress { get; init; }

        /// <inheritdoc/>
        public INetwork Network { get; internal set; }
    }
}
