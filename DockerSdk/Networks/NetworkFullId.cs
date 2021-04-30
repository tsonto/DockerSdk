using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace DockerSdk.Networks
{
    /// <summary>
    /// Represents a Docker networks's full-length ID.
    /// </summary>
    public record NetworkFullId : NetworkId
    {
        internal NetworkFullId(string id) : base(id) { }

        /// <summary>
        /// Tries to parse the input as a full-length Docker network ID.
        /// </summary>
        /// <param name="input">The text to parse.</param>
        /// <param name="id">The resultant ID, or null if parsing failed.</param>
        /// <returns>True if parsing succeeded; false otherwise.</returns>
        /// <exception cref="ArgumentNullException">The input is null.</exception>
        public static bool TryParse(string input, [NotNullWhen(returnValue: true)] out NetworkFullId? id)
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));

            if (regex.IsMatch(input))
            {
                id = new NetworkFullId(input);
                return true;
            }
            else
            {
                id = null;
                return false;
            }
        }

        /// <inheritdoc/>
        public override string ToString() => value;

        /// <summary>
        /// Parses the input as a full-length Docker network ID.
        /// </summary>
        /// <param name="input">The text to parse.</param>
        /// <returns>The resultant ID.</returns>
        /// <exception cref="MalformedReferenceException">
        /// The input is not a validly-formatted full-length network ID.
        /// </exception>
        /// <exception cref="ArgumentNullException">The input is null.</exception>
        public static new NetworkFullId Parse(string input)
            => TryParse(input, out NetworkFullId? id)
            ? id
            : throw new MalformedReferenceException($"\"{input}\" is not a valid network ID.");

        private static readonly Regex regex = new Regex(
            @"^
                (
                    [0-9a-f]{64}    # Docker's libnetwork uses this form for locally-scoped networks
                |
                    [0-9a-z]{25}    # Docker's libnetwork uses this form for swarm-scoped networks
                )
            $",
            RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.IgnorePatternWhitespace | RegexOptions.CultureInvariant);
    }
}
