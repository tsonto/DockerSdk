using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using DockerSdk.Containers.Dto;

namespace DockerSdk.Images.Dto
{
    internal class ImageInspectResponse
    {
        public string Id { get; set; } = null!;

        public IList<string>? RepoTags { get; set; }

        public IList<string>? RepoDigests { get; set; }

        public string? Parent { get; set; }

        public string? Comment { get; set; }

        public DateTimeOffset Created { get; set; }

        public string? Container { get; set; }

        public ContainerConfig? ContainerConfig { get; set; }

        public string? DockerVersion { get; set; }

        public string? Author { get; set; }

        public ContainerConfig? Config { get; set; }

        public string? Architecture { get; set; }

        public string? Variant { get; set; }

        [JsonPropertyName("Os")]
        public string OS { get; set; } = null!;

        [JsonPropertyName("OsVersion")]
        public string? OSVersion { get; set; }

        public long Size { get; set; }

        public long VirtualSize { get; set; }

        public GraphDriverData? GraphDriver { get; set; }

        public RootFS? RootFS { get; set; }

        public ImageMetadata? Metadata { get; set; }
    }
}
