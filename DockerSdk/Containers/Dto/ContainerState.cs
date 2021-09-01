using System.Text.Json.Serialization;

namespace DockerSdk.Containers.Dto
{
    internal class ContainerState
    {
        public bool Dead { get; set; }
        public string? Error { get; set; }
        public long ExitCode { get; set; }
        public string? FinishedAt { get; set; }
        public Health? Health { get; set; }

        [JsonPropertyName("OOMKilled")]
        public bool OomKilled { get; set; }

        public bool Paused { get; set; }
        public long Pid { get; set; }
        public bool Restarting { get; set; }
        public bool Running { get; set; }
        public string? StartedAt { get; set; }
        public string Status { get; set; } = null!;
    }
}
