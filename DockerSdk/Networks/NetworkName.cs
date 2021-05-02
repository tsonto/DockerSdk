using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace DockerSdk.Networks
{
    /// <summary>
    /// Represents a Docker network's name.
    /// </summary>
    public record NetworkName : NetworkReference
    {
        internal NetworkName(string name) : base(name) { }

        private static readonly Regex regex = new Regex(
            @"^
                # Presently (as of 2021-04-27) Docker's libnetwork imposes no restrictions on the
                # name of swarm-scoped networks except that it has at least one non-whitespace
                # character. Even newline characters are accepted. Since the names of
                # locally-scoped networks are a proper subset of those names, we only need to check
                # the one pattern.
                (?<name>
                    [^\0]*\S[^\0]*      # Any non-null characters are allowed, including control characters, as long as there's at least one non-space character.
                )
                (\0.*)?                 # If there's a null character, discard it and everything that follows.
            $",
            RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.IgnorePatternWhitespace | RegexOptions.CultureInvariant | RegexOptions.Singleline);

        /// <summary>
        /// Tries to parse the given input as a Docker network name.
        /// </summary>
        /// <param name="input">The string to parse.</param>
        /// <param name="name">The name, or null if parsing failed.</param>
        /// <returns>True if parsing succeeded; false otherwise.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="input"/> is null.</exception>
        public static bool TryParse(string input, [NotNullWhen(returnValue: true)] out NetworkName? name)
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));

            var match = regex.Match(input);
            if (match.Success)
            {
                name = new NetworkName(match.Groups["name"].Value);
                return true;
            }
            else
            {
                name = null;
                return false;
            }
        }

        /// <inheritdoc/>
        public override string ToString() => value;

        /// <summary>
        /// Parses the given input as a Docker network name.
        /// </summary>
        /// <param name="input">The string to parse.</param>
        /// <returns>The name.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="input"/> is null.</exception>
        /// <exception cref="MalformedReferenceException">The input could not be parsed as a network name.</exception>
        public static new NetworkName Parse(string input)
            => TryParse(input, out var name)
            ? name
            : throw new MalformedReferenceException($"\"{input}\" is not a valid network name.");
    }
}
