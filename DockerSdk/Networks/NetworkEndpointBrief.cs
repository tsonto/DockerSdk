using System.Net;
using System.Net.NetworkInformation;
using DockerSdk.Containers;
using CoreModels = Docker.DotNet.Models;

namespace DockerSdk.Networks
{
    public class NetworkEndpoint : NetworkEndpointBrief
    {
        internal NetworkEndpoint(DockerClient docker, CoreModels.EndpointSettings raw, ContainerFullId containerId)
            : base(docker, raw, containerId)
        {
            if (!string.IsNullOrEmpty(raw.Gateway))
                IPv4Gateway = IPAddress.Parse(raw.Gateway);

            if (!string.IsNullOrEmpty(raw.IPv6Gateway))
                IPv6Gateway = IPAddress.Parse(raw.IPv6Gateway);
        }

        public IPAddress IPv4Gateway { get; }

        public IPAddress IPv6Gateway { get; }
    }

    /// <summary>
    /// Provides information about a Docker network endpoint. An endpoint is a connection between a Docker container and
    /// a Docker network.
    /// </summary>
    /// <remarks>This class holds a snapshot in time. Its information is immutable once created.</remarks>
    public class NetworkEndpointBrief
    {
        /// <summary>
        /// Build the object based on what we get from an inspect-container response. (Ironically, that has more network
        /// information than an inspect-network response.)
        /// </summary>
        internal NetworkEndpointBrief(DockerClient docker, CoreModels.EndpointSettings raw, ContainerFullId containerId)
        {
            Container = new Container(docker, containerId);
            EndpointId = raw.EndpointID;
            Network = new Network(docker, new NetworkFullId(raw.NetworkID));

            if (!string.IsNullOrEmpty(raw.IPAddress))
            {
                IPv4Address = IPAddress.Parse(raw.IPAddress);
                IPv4Subnet = new IPSubnet(IPv4Address, (int)raw.IPPrefixLen);
            }
            if (IPSubnet.TryParse(raw.IPAddress, out IPSubnet? ipv4Subnet, out IPAddress? ipv4Address))
            {
                IPv6Address = IPAddress.Parse(raw.GlobalIPv6Address);
                IPv6Subnet = new IPSubnet(IPv6Address, (int)raw.GlobalIPv6PrefixLen);
            }

            if (!string.IsNullOrEmpty(raw.MacAddress) && PhysicalAddress.TryParse(raw.MacAddress, out var mac))
                MacAddress = mac;
            else
                throw new DockerException($"Network endpoint {EndpointId} is missing its MAC address.");
        }

        /// <summary>
        /// Build the object based on what we get from an inspect-network response. (Ironically, we get less network
        /// information from that than with an inspect-container response.)
        /// </summary>
        internal NetworkEndpointBrief(CoreModels.EndpointResource raw, Container container, Network network)
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
