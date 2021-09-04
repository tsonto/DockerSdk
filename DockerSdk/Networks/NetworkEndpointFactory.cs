using System.Net;
using System.Net.NetworkInformation;
using DockerSdk.Containers;
using DockerSdk.Networks.Dto;

namespace DockerSdk.Networks
{
    internal static class NetworkEndpointFactory
    {
        /// <summary>
        /// Build the object based on what we get from an inspect-network response.
        /// </summary>
        public static INetworkEndpoint Create(DockerClient docker, ContainerFullId containerId, EndpointResource raw, INetwork network)
        {
            return new NetworkEndpoint(raw.EndpointId!, network, new Container(docker, containerId))
            {
                IPv4Address = TryParseIP(raw.IPv4Address),
                IPv6Address = TryParseIP(raw.IPv6Address),
                MacAddress = PhysicalAddress.Parse(raw.MacAddress),
            };
        }

        /// <summary>
        /// Build the object based on what we get from an inspect-container response.
        /// </summary>
        internal static INetworkEndpoint Create(DockerClient docker, EndpointSettings raw, IContainer container)
        {
            return new NetworkEndpoint(raw.EndpointId, new Network(docker, new NetworkFullId(raw.NetworkId)), container)
            {
                IPv4Address = TryParseIP(raw.IPAddress),
                IPv6Address = TryParseIP(raw.GlobalIPv6Address),
                MacAddress = PhysicalAddress.Parse(raw.MacAddress),
            };
        }

        private static IPAddress? TryParseIP(string? input)
            => string.IsNullOrEmpty(input)
            ? null
            : IPAddress.Parse(input.Split('/')[0]);
    }
}
