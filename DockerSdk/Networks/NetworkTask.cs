using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DockerSdk.Networks
{
    internal class NetworkTask
    {
        [JsonPropertyName("EndpointID")]
        public string? EndpointId { get; set; }

        public string? EndpointIP { get; set; }
        public IDictionary<string, string>? Info { get; set; }
        public string? Name { get; set; }
    }
}
