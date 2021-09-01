using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DockerSdk.Networks
{
    public class Ipam
    {
        public string Driver { get; set; } = null!;

        public IDictionary<string, string>? Options { get; set; }

        public IList<IpamConfig>? Config { get; set; }
    }
}
