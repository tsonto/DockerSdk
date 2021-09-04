using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DockerSdk.Containers.Dto
{
    internal class DeviceRequest
    {
        public string? Driver { get; set; }

        public long? Count { get; set; }

        [JsonPropertyName("DeviceIDs")]
        public string[]? DeviceIds { get; set; }

        public string[][]? Capabilities { get; set; }

        public IDictionary<string, string>? Options { get; set; }
    }
}
