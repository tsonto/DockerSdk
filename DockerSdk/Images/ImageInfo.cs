using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace DockerSdk.Images
{
    /// <remarks>This class holds a snapshot in time. Its information is immutable once created.</remarks>
    internal class ImageInfo : Image, IImageInfo
    {
        internal ImageInfo(DockerClient client, ImageFullId id)
            : base(client, id)
        {
        }

        /// <inheritdoc/>
        public string Author { get; init; } = "";

        /// <inheritdoc/>
        public string Comment { get; init; } = "";

        /// <inheritdoc/>
        public DateTimeOffset CreationTime { get; init; }

        /// <inheritdoc/>
        public string? Digest { get; init; }

        /// <inheritdoc/>
        public IReadOnlyDictionary<string, string> Labels { get; init; } = ImmutableDictionary<string, string>.Empty;

        /// <inheritdoc/>
        public GuestOsType OsType { get; init; }

        /// <inheritdoc/>
        public IImage? ParentImage { get; init; }

        /// <inheritdoc/>
        public long Size { get; init; }

        /// <inheritdoc/>
        public IReadOnlyList<ImageName> Tags { get; init; } = ImmutableList<ImageName>.Empty;

        /// <inheritdoc/>
        public long VirtualSize { get; init; }

        /// <inheritdoc/>
        public string WorkingDirectory { get; init; } = "";

        // TODO: Container and ContainerConfig -- these represent the temporary container created when building the image
        // TODO: DockerVersion -- the version of Docker that created the image
        // TODO: Config - This is what the ID is based on
        // TODO: Architecture
        // TODO: Variant
        // TODO: Os
        // TODO: OsVersion
        // TODO: GraphicDriver
        // TODO: RootFS
        // TODO: ImageMetadata
    }
}
