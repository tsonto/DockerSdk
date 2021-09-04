using System.Text.Json.Serialization;

namespace DockerSdk.Networks
{
    internal class EndpointResource
    {
        [JsonPropertyName("EndpointID")]
        public string? EndpointId { get; set; }

        public string? IPv4Address { get; set; }
        public string? IPv6Address { get; set; }
        public string? MacAddress { get; set; }
        public string Name { get; set; } = null!;
    }
}
