using System.Diagnostics.CodeAnalysis;

namespace DockerSdk.Images
{
    /// <summary>
    /// Represents a Docker image's full-length ID, including the "sha256:" prefix.
    /// </summary>
    public record ImageFullId : ImageId
    {
        /// <summary>
        /// Tries to parse the input as a full-length Docker image ID.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool TryParse(string input, [NotNullWhen(returnValue: true)] out ImageFullId? id)
        {
            if (TryParse(input, out ImageReference? reference) && reference is ImageFullId x)
            {
                id = x;
                return true;
            }
            else
            {
                id = null;
                return false;
            }
        }

        /// <summary>
        /// Parses the input as a full-length Docker image ID.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        /// <exception cref="MalformedReferenceException">
        /// The input is not a validly-formatted full-length image ID.
        /// </exception>
        public static new ImageFullId Parse(string input)
            => TryParse(input, out ImageFullId? id)
            ? id
            : throw new MalformedReferenceException($"\"{input}\" is not a valid full-length image ID.");

        internal ImageFullId(string id) : base(id) { }

        /// <summary>
        /// Gets the short form of the ID.
        /// </summary>
        public ImageId ShortForm => new(Shorten(_value));

        /// <inheritdoc/>
        public override string ToString() => _value;
    }
}
