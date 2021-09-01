using System.Collections.Generic;

namespace DockerSdk.Networks
{
    public class IpamConfig
    {
        public IDictionary<string, string>? AuxiliaryAddresses { get; set; }
        public string? Gateway { get; set; }
        public string? IPRange { get; set; }
        public string? Subnet { get; set; }
    }
}
