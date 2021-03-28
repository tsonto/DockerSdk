using System;
using System.Diagnostics.CodeAnalysis;

namespace DockerSdk.Images
{
    /// <summary>
    /// Represents the various components of a Docker image's name or ID.
    /// </summary>
    internal record DecomposedImageReference
    {
        internal DecomposedImageReference(string full)
            => Full = full;

        /// <summary>
        /// Gets the full text of the reference.
        /// </summary>
        public string Full { get; }

        /// <summary>
        /// Gets the hostname, if present.
        /// </summary>
        public string? Host { get; internal init; }

        /// <summary>
        /// Gets the port number, if present.
        /// </summary>
        public int? Port { get; internal init; }

        /// <summary>
        /// Gets the hostname with port. If the hostname was not present, this is null. Otherwise, if the port was not
        /// present, this is just the hostname.
        /// </summary>
        public string? HostWithPort { get; internal init; }

        /// <summary>
        /// Gets the repository, if present.
        /// </summary>
        public string? Repository { get; internal init; }

        /// <summary>
        /// Gets the repository, if present, without the host portion.
        /// </summary>
        public string? RepositoryWithoutHost { get; internal init; }

        /// <summary>
        /// Gets the tag, if present.
        /// </summary>
        /// <remarks>Docker ignores the tag if the digest is set.</remarks>
        public string? Tag { get; internal init; }

        /// <summary>
        /// Gets the digest, if present. This includes the "sha256:" prefix.
        /// </summary>
        public string? Digest { get; internal init; }

        /// <summary>
        /// Gets the full-length ID, if present. This includes the "sha256:" prefix.
        /// </summary>
        public string? LongId { get; internal init; }

        /// <summary>
        /// Gets the shortened ID, if an ID was present at all.
        /// </summary>
        public string? ShortId { get; internal init; }

        /// <summary>
        /// Gets a value indicating whether the reference is an image ID.
        /// </summary>
        public bool IsId => ShortId is not null;

        /// <summary>
        /// Gets a value indicating whether the reference is a full-length image ID.
        /// </summary>
        public bool IsFullId => LongId is not null;

        /// <summary>
        /// Gets a value indicating whether the reference is an image name.
        /// </summary>
        public bool IsName => Repository is not null;

        /// <inheritdoc/>
        public override string ToString() => Full;

        /// <summary>
        /// Creates an image reference from the parsed values.
        /// </summary>
        /// <returns>An <see cref="ImageReference"/> object.</returns>
        public ImageReference ToReference()
        {
            if (IsFullId)
                return new ImageFullId(LongId!);
            else if (IsId)
                return new ImageId(ShortId!);
            else if (IsName)
                return new ImageName(Full, Repository!, Tag, Digest);
            else
                throw new InvalidOperationException("Parsed the image reference, but it is neither an ID nor a name.");
        }
    }
}
