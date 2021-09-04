using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DockerSdk.Containers.Dto
{
    // TODO: is this actually a swarm model?
    internal class ContainerNode
    {
        [JsonPropertyName("Addr")]
        public string? Address { get; set; }

        public long? Cpus { get; set; }

        [JsonPropertyName("ID")]
        public string Id { get; set; } = null!;

        [JsonPropertyName("IP")]
        public string? IPAddress { get; set; }

        public IDictionary<string, string>? Labels { get; set; }
        public long? Memory { get; set; }
        public string? Name { get; set; }
    }
}
