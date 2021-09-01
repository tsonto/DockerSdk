using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace DockerSdk.Networks.Dto
{
    internal class NetworkSettings 
    {
        /// <summary>
        /// Name of the network's bridge (for example, <c>docker0</c>).
        /// </summary>
        public string Bridge { get; set; } = null!;

        /// <summary>
        /// SandboxID uniquely represents a container's network stack.
        /// </summary>
        [JsonPropertyName("SandboxID")]
        public string SandboxId { get; set; } = null!;

        /// <summary>
        /// Indicates if hairpin NAT should be enabled on the virtual interface.
        /// </summary>
        public bool HairpinMode { get; set; }

        /// <summary>
        /// IPv6 unicast address using the link-local prefix.
        /// </summary>
        public string? LinkLocalIPv6Address { get; set; }

        /// <summary>
        /// Prefix length of the IPv6 unicast address.
        /// </summary>
        public long? LinkLocalIPv6PrefixLen { get; set; }

        /// <summary>
        /// PortMap describes the mapping of container ports to host ports, using the container's port-number and protocol as key in the format &lt;port&gt;/&lt;protocol&gt;, for example, 80/udp. 
        /// If a container's port is mapped for multiple protocols, separate entries are added to the mapping table.
        /// </summary>
        public IDictionary<string, IList<PortBinding>> Ports { get; set; } = null!;

        /// <summary>
        /// SandboxKey identifies the sandbox
        /// </summary>
        public string SandboxKey { get; set; } = null!;

        public IList<Address>? SecondaryIPAddresses { get; set; }

        public IList<Address>? SecondaryIPv6Addresses { get; set; }

        /// <summary>
        /// Information about all networks that the container is connected to.
        /// </summary>
        public IDictionary<string, EndpointSettings> Networks { get; set; } = null!;
    }
}
