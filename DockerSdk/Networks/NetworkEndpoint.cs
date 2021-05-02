using System.Net;
using System.Net.NetworkInformation;
using DockerSdk.Containers;
using CoreModels = Docker.DotNet.Models;

namespace DockerSdk.Networks
{
    /// <summary>
    /// Provides information about a Docker network endpoint. An endpoint is a connection between a Docker container and a Docker network.
    /// </summary>
    /// <remarks>This class holds a snapshot in time. Its information is immutable once created.</remarks>
    public sealed class NetworkEndpoint
    {
        internal NetworkEndpoint(CoreModels.EndpointResource raw, Container container, Network network)
        {
            Container = container;
            EndpointId = raw.EndpointID;
            Network = network;

            if (IPSubnet.TryParse(raw.IPv4Address, out IPSubnet? ipv4Subnet, out IPAddress? ipv4Address))
            {
                IPv4Address = ipv4Address;
                IPv4Subnet = ipv4Subnet;    
            }

            if (IPSubnet.TryParse(raw.IPv6Address, out IPSubnet? ipv6Subnet, out IPAddress? ipv6Address))
            {
                IPv6Address = ipv6Address;
                IPv6Subnet = ipv6Subnet;
            }

            if (!string.IsNullOrEmpty(raw.MacAddress) && PhysicalAddress.TryParse(raw.MacAddress, out var mac))
                MacAddress = mac;
            else
                throw new DockerException($"Network endpoint {EndpointId} is missing its MAC address.");
        }

        /// <summary>
        /// Gets the Docker container that this endpoint connects to.
        /// </summary>
        public Container Container { get; }

        /// <summary>
        /// Gets the endpoint's ID.
        /// </summary>
        public string EndpointId { get; }

        /// <summary>
        /// Gets the container's IPv4 address (if it has one) on this endpoint's network.
        /// </summary>
        public IPAddress? IPv4Address { get; }
        
        /// <summary>
        /// Gets the IPv4 subnet (if applicable) of this endpoint's network.
        /// </summary>
        public IPSubnet? IPv4Subnet { get; }

        /// <summary>
        /// Gets the container's IPv6 address (if it has one) on this endpoint's network.
        /// </summary>
        public IPAddress? IPv6Address { get; }

        /// <summary>
        /// Gets the IPv6 subnet (if applicable) of this endpoint's network.
        /// </summary>
        public IPSubnet? IPv6Subnet { get; }

        /// <summary>
        /// Gets the container's MAC address on this endpoint's network.
        /// </summary>
        public PhysicalAddress MacAddress { get; }

        /// <summary>
        /// Gets the Docker network that this endpoint connects to.
        /// </summary>
        public Network Network { get; }
    }
}
