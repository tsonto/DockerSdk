using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace DockerSdk.Volumes
{
    /// <inheritdoc/>
    internal class VolumeInfo : Volume, IVolumeInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VolumeInfo"/> type.
        /// </summary>
        /// <param name="client">The <see cref="DockerClient"/> instance to use.</param>
        /// <param name="name">The volume's name.</param>
        public VolumeInfo(DockerClient client, VolumeName name)
            : base(client, name)
        { }

        /// <inheritdoc/>
        public DateTimeOffset CreationTime { get; init; }

        /// <inheritdoc/>
        public string Driver { get; init; } = null!;

        /// <inheritdoc/>
        public IReadOnlyDictionary<string, string> Labels { get; init; } = ImmutableDictionary<string, string>.Empty;

        /// <inheritdoc/>
        public string Mountpoint { get; init; } = null!;

        /// <inheritdoc/>
        public VolumeScope Scope { get; init; }
    }
}
