using System.Diagnostics.CodeAnalysis;

namespace DockerSdk.Images
{
    /// <summary>
    /// Represents a Docker image's ID in either short form or long form.
    /// </summary>
    public record ImageId : ImageReference
    {
        /// <summary>
        /// Tries to parse the input as a Docker image ID.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool TryParse(string input, [NotNullWhen(returnValue: true)] out ImageId? id)
        {
            if (TryParse(input, out ImageReference? reference) && reference is ImageId x)
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
        /// <exception cref="MalformedReferenceException">The input is not a validly-formatted image ID.</exception>
        public static new ImageId Parse(string input)
            => TryParse(input, out ImageId? id)
            ? id
            : throw new MalformedReferenceException($"\"{input}\" is not a valid image ID.");

        internal ImageId(string id) : base(id) { }

        /// <inheritdoc/>
        public override string ToString() => _value;

        /// <summary>
        /// Given an image ID, produces the short form of the image ID.
        /// </summary>
        /// <param name="id">The full ID or short ID.</param>
        /// <returns>The short image ID.</returns>
        /// <remarks>This does not necessarily fully validate the input.</remarks>
        /// <exception cref="MalformedReferenceException">The input is not a validly-formatted image ID.</exception>
        public static string Shorten(string id)
        {
            string result = id;

            if (result.Contains(':'))
                result = result.Split(':')[1];

            if (result.Length == 64)
                return result.Substring(0, 12);
            else if (result.Length == 12)
                return result;
            else
                throw new MalformedReferenceException($"{id} is not a valid image ID.");
        }

    }
}
