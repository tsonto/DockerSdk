using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace DockerSdk.Containers
{
    /// <summary>
    /// Represents a Docker container's ID in either short form or long form.
    /// </summary>
    public record ContainerId : ContainerReference
    {
        internal ContainerId(string id) : base(id) { }

        /// <summary>
        /// Given a container ID, produces the short form of the container ID.
        /// </summary>
        /// <param name="id">The full ID or short ID.</param>
        /// <returns>The short container ID.</returns>
        /// <remarks>This does not necessarily fully validate the input.</remarks>
        /// <exception cref="MalformedReferenceException">The input is not a validly-formatted container ID.</exception>
        public static string Shorten(string id) => id.Substring(0, 12);

        /// <inheritdoc/>
        public override string ToString() => _value;

        /// <summary>
        /// Parses the input as a Docker container ID.
        /// </summary>
        /// <param name="input">The text to parse.</param>
        /// <returns>The reference.</returns>
        /// <exception cref="MalformedReferenceException">
        /// The input is not a validly-formatted container ID.
        /// </exception>
        public static new ContainerId Parse(string input)
            => TryParse(input, out ContainerId? id)
            ? id
            : throw new MalformedReferenceException($"\"{input}\" is not a valid container ID.");

        /// <summary>
        /// Tries to parse the input as a Docker container ID.
        /// </summary>
        /// <param name="input">The string to parse.</param>
        /// <param name="id">The ID, or null if parsing failed.</param>
        /// <returns>True if the input is well-formed as a container ID; false otherwise.</returns>
        public static bool TryParse(string input, [NotNullWhen(returnValue:true)] out ContainerId? id)
        {
            if (ContainerFullId.TryParse(input, out ContainerFullId? fullId))
            {
                id = fullId;
                return true;
            }
            
            if (_shortIdRegex.IsMatch(input))
            {
                id = new (input);
                return true;
            }
            else
            {
                id = null;
                return false;
            }
        }

        private static readonly Regex _shortIdRegex = new(
            @"^
                [a-f0-9]{12}    # Exactly 12 lowercase hex digits
            $",
            RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.IgnorePatternWhitespace);
    }
}
