using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace DockerSdk.Volumes
{
    internal class VolumeInfo : Volume, IVolumeInfo
    {
        public VolumeInfo(DockerClient client, VolumeName name)
            : base(client, name)
        { }

        public DateTimeOffset CreationTime { get; init; }
        public string Driver { get; init; } = null!;
        public IReadOnlyDictionary<string, string> Labels { get; init; } = ImmutableDictionary<string, string>.Empty;
        public string Mountpoint { get; init; } = null!;
        public VolumeScope Scope { get; init; }
    }
}
