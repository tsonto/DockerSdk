using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace DockerSdk.Networks
{
    /// <summary>
    /// Represents a Docker networks's ID, in either short form or long form.
    /// </summary>
    public record NetworkId : NetworkReference
    {
        internal NetworkId(string id) : base(id) { }

        /// <inheritdoc/>
        public override string ToString() => value;

        /// <summary>
        /// Parses the input as a Docker network ID.
        /// </summary>
        /// <param name="input">The text to parse.</param>
        /// <returns>The resultant ID.</returns>
        /// <exception cref="MalformedReferenceException">
        /// The input is not a validly-formatted network ID.
        /// </exception>
        /// <exception cref="ArgumentNullException">The input is null.</exception>
        public static new NetworkId Parse(string input)
            => TryParse(input, out var id)
            ? id
            : throw new MalformedReferenceException($"\"{id}\" is not a valid network ID.");

        /// <summary>
        /// Tries to parse the input as a Docker network ID.
        /// </summary>
        /// <param name="input">The text to parse.</param>
        /// <param name="id">The resultant ID, or null if parsing failed.</param>
        /// <returns>True if parsing succeeded; false otherwise.</returns>
        /// <exception cref="ArgumentNullException">The input is null.</exception>
        public static bool TryParse(string input, [NotNullWhen(returnValue: true)] out NetworkId? id)
        {
            if (NetworkFullId.TryParse(input, out var fullId))
            {
                id = fullId;
                return true;
            }
            else if (regex.IsMatch(input))
            {
                id = new NetworkId(input);
                return true;
            }
            else
            {
                id = null;
                return false;
            }
        }

        private static readonly Regex regex = new Regex(
            @"^
                [0-9a-z]{12}    # Short IDs for swarm-scoped networks are a proper superset of short IDs for locally-scoped, so we only need to check the former pattern
            $",
            RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.IgnorePatternWhitespace | RegexOptions.CultureInvariant);
    }
}
