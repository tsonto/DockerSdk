using System.Collections.Generic;

namespace DockerSdk.Networks.Dto
{
    internal class SummaryNetworkSettings
    {
        public IDictionary<string, EndpointSettings>? Networks { get; set; }
    }
}
