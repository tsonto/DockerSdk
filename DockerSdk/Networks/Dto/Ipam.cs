using System.Collections.Generic;

namespace DockerSdk.Networks.Dto
{
    internal class Ipam
    {
        public string Driver { get; set; } = null!;

        public Dictionary<string, string>? Options { get; set; }

        public IpamConfig[]? Config { get; set; }
    }
}
