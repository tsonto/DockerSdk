using System.Collections.Generic;

namespace DockerSdk.Networks.Dto
{
    internal class EndpointIpamConfig
    {
        public string? IPv4Address { get; set; }

        public string? IPv6Address { get; set; }

        public IList<string>? LinkLocalIPs { get; set; }
    }
}
