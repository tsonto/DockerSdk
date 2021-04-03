using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace DockerSdk.Containers
{
    /// <summary>
    /// Represents a Docker container's name.
    /// </summary>
    /// <remarks>
    /// Container names are case-sensitive. The must be at least two characters long. The first character must be
    /// alphanumeric (ASCII character set), and subsequent characters may be alphanumeric, underscore, hyphen, or
    /// period.
    /// </remarks>
    public record ContainerName : ContainerReference
    {
        internal ContainerName(string name)
            : base(RemoveLeadingSlash(name))
        { }

        private static string RemoveLeadingSlash(string input)
            => input.StartsWith('/')
            ? input.Substring(1)
            : input;

        /// <inheritdoc/>
        public override string ToString() => _value;

        /// <summary>
        /// Tries to parse the given input as a Docker container name.
        /// </summary>
        /// <param name="input">The string to parse.</param>
        /// <param name="name">The name, or null if parsing failed.</param>
        /// <returns>True if parsing succeeded; false otherwise.</returns>
        /// <exception cref="ArgumentException"><paramref name="input"/> is null or empty.</exception>
        public static bool TryParse(string input, [NotNullWhen(returnValue: true)] out ContainerName? name)
        {
            input = RemoveLeadingSlash(input);

            if (_nameRegex.IsMatch(input))
            {
                name = new(input);
                return true;
            }
            else
            {
                name = null;
                return false;
            }
        }

        private static readonly Regex _nameRegex = new(
            @"^
                [a-zA-Z0-9]         # Must start with an alphanum character
                [a-zA-Z0-9_.-]+     # Followed by at least one alphanum, underscore, period, or hyphen
            $",
            RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.IgnorePatternWhitespace);
    }
}
