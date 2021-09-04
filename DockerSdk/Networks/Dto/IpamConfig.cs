using System.Collections.Generic;

namespace DockerSdk.Networks.Dto
{
    internal class IpamConfig
    {
        public Dictionary<string, string>? AuxiliaryAddresses { get; set; }
        public string? Gateway { get; set; }
        public string? IPRange { get; set; }
        public string? Subnet { get; set; }
    }
}
