using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DockerSdk.Networks
{
    internal class ServiceInfo
    {
        public long? LocalLBIndex { get; set; }

        public IList<string>? Ports { get; set; }

        public IList<NetworkTask>? Tasks { get; set; }

        [JsonPropertyName("VIP")]
        public string? Vip { get; set; }
    }
}
