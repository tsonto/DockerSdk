using System.Text.Json.Serialization;

namespace DockerSdk.Networks.Dto
{
    internal class PortBinding
    {
        [JsonPropertyName("HostIp")]
        public string HostIP { get; set; } = null!;

        public string HostPort { get; set; } = null!;
    }
}
