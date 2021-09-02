using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using DockerSdk.JsonConverters;

namespace DockerSdk.Images.Dto
{
    internal class ImagesListResponse
    {
        public long? Containers { get; set; }

        [JsonConverter(typeof(UnixEpochConverter))]
        public DateTimeOffset Created { get; set; }

        public string Id { get; set; } = null!;

        public IDictionary<string, string>? Labels { get; set; }

        public string? ParentId { get; set; }

        public string[]? RepoDigests { get; set; }

        public string[]? RepoTags { get; set; }

        public long SharedSize { get; set; }

        public long Size { get; set; }

        public long VirtualSize { get; set; }
    }
}
