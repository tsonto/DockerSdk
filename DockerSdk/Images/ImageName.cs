using System.Diagnostics.CodeAnalysis;

namespace DockerSdk.Images
{
    /// <summary>
    /// Represents an image name.
    /// </summary>
    public record ImageName : ImageReference
    {
        /// <summary>
        /// Gets the repository part of the name.
        /// </summary>
        public string Repository { get; }

        /// <summary>
        /// Gets the tag part of the name, if present.
        /// </summary>
        public string? Tag { get; }

        /// <summary>
        /// Gets the digest part of the name, if present.
        /// </summary>
        public string? Digest { get; }

        internal ImageName(string name, string repository, string? tag, string? digest) 
            : base(name) 
        {
            Repository = repository;
            Tag = digest is null ? tag : null;  // Docker ignores the tag if the digest is set, so it's misleading to show both
            Digest = digest;
        }

        /// <summary>
        /// Tries to parse the given input as a Docker image name.
        /// </summary>
        /// <param name="input">The string to parse.</param>
        /// <param name="name">The name, or null if parsing failed.</param>
        /// <returns>True if parsing succeeded; false otherwise.</returns>
        public static bool TryParse(string input, [NotNullWhen(returnValue: true)] out ImageName? name)
        {
            if (TryParse(input, out ImageReference? reference) && reference is ImageName x)
            {
                name = x;
                return true;
            }
            else
            {
                name = null;
                return false;
            }
        }

        /// <summary>
        /// Parses the given input as a Docker image name.
        /// </summary>
        /// <param name="input">The string to parse.</param>
        /// <returns>The name.</returns>
        /// <exception cref="MalformedReferenceException">The input could not be parsed as an image name.</exception>
        public static new ImageName Parse(string input)
            => TryParse(input, out ImageName? name)
            ? name
            : throw new MalformedReferenceException($"\"{input}\" is not a valid image name.");

        /// <inheritdoc/>
        public override string ToString() => _value;
    }
}
