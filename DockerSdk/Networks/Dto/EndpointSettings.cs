using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DockerSdk.Networks.Dto
{
    internal class EndpointSettings
    {
        /// <summary>
        /// EndpointIPAMConfig represents an endpoint's IPAM configuration.
        /// </summary>
        [JsonPropertyName("IPAMConfig")]
        public EndpointIpamConfig? IpamConfig { get; set; }

        public IList<string>? Links { get; set; }

        public IList<string>? Aliases { get; set; }

        /// <summary>
        /// Unique ID of the network.
        /// </summary>
        [JsonPropertyName("NetworkID")]
        public string NetworkId { get; set; } = null!;

        /// <summary>
        /// Unique ID for the service endpoint in a Sandbox.
        /// </summary>
        [JsonPropertyName("EndpointID")]
        public string EndpointId { get; set; } = null!;

        /// <summary>
        /// Gateway address for this network.
        /// </summary>
        public string? Gateway { get; set; }

        /// <summary>
        /// IPv4 address.
        /// </summary>
        public string? IPAddress { get; set; }

        /// <summary>
        /// Mask length of the IPv4 address.
        /// </summary>
        public long? IPPrefixLen { get; set; }

        /// <summary>
        /// IPv6 gateway address.
        /// </summary>
        public string? IPv6Gateway { get; set; }

        /// <summary>
        /// Global IPv6 address.
        /// </summary>
        public string? GlobalIPv6Address { get; set; }

        /// <summary>
        /// Mask length of the global IPv6 address.
        /// </summary>
        public long? GlobalIPv6PrefixLen { get; set; }

        /// <summary>
        /// MAC address for the endpoint on this network.
        /// </summary>
        public string? MacAddress { get; set; }

        /// <summary>
        /// DriverOpts is a mapping of driver options and values. These options are passed directly to the driver and are driver specific.
        /// </summary>
        public IDictionary<string, string>? DriverOpts { get; set; }
    }
}
