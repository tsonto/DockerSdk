using System.Collections.Generic;

namespace DockerSdk.Networks.Dto
{
    internal class NetworkingConfig
    {
        public IDictionary<string, EndpointSettings>? EndpointsConfig { get; set; }
    }
}
