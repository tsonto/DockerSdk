using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DockerSdk.Events.Dto
{
    internal class Actor
    {
        [JsonPropertyName("ID")]
        public string Id { get; set; } = null!;

        public IDictionary<string, string> Attributes { get; set; } = null!;
    }
}
