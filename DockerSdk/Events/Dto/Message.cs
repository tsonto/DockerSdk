using System.Text.Json.Serialization;

namespace DockerSdk.Events.Dto
{
    internal class Message
    {
        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("from")]
        public string? From { get; set; }

        public string? Type { get; set; }

        public string Action { get; set; } = null!;

        public Actor Actor { get; set; } = null!;

        [JsonPropertyName("scope")]
        public string? Scope { get; set; }

        [JsonPropertyName("time")]
        public long Time { get; set; }

        [JsonPropertyName("timeNano")]
        public long TimeNano { get; set; }
    }
}
