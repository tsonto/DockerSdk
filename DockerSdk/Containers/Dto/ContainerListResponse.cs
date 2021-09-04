using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using DockerSdk.JsonConverters;
using DockerSdk.Networks.Dto;
using DockerSdk.Volumes.Dto;

namespace DockerSdk.Containers.Dto
{
    internal class ContainerListResponse
    {
        public string Id { get; set; } = null!;

        public IList<string>? Names { get; set; }

        public string? Image { get; set; }

        [JsonPropertyName("ImageID")]
        public string? ImageId { get; set; }

        public string? Command { get; set; }

        [JsonConverter(typeof(UnixEpochConverter))]
        public DateTimeOffset? Created { get; set; }

        public IList<Port>? Ports { get; set; }

        public long? SizeRw { get; set; }

        [JsonPropertyName("SizeRootFs")]
        public long? SizeRootFS { get; set; }

        public IDictionary<string, string>? Labels { get; set; }

        public string? State { get; set; }

        public string? Status { get; set; }

        public SummaryNetworkSettings? NetworkSettings { get; set; }

        public IList<MountPoint>? Mounts { get; set; }
    }
}
