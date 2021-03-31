using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.RegularExpressions;

namespace DockerSdk.Registries
{
    /// <summary>
    /// Represents a reference to a Docker registry. A registry is a site that hosts Docker images.
    /// </summary>
    public record RegistryReference
    {
        internal RegistryReference(string value)
        {
            _value = value;
        }

        private static readonly Regex _validationRegex = new(
            @"^
                (?<host>                # Capture the first part as 'host'.
                    (                   # We always require at least one label.
                        [a-zA-Z0-9]     # The label must start with an ASCII alphanumeric character.
                        [a-zA-Z0-9-]*   # May be followed by any number of ASCII alphanumeric characters or hyphens.
                    )
                    (                   # May optionally be followed zero or more additional labels separated by periods.
                        \.
                        [a-zA-Z0-9]     # The label must start with an ASCII alphanumeric character.
                        [a-zA-Z0-9-]*   # May be followed by any number of ASCII alphanumeric characters or hyphens.
                    )*
                )
                (                       # May optionally be followed by a colon and one or more digits.
                    \:
                    (?<port>            # Capture the digits as 'port'.
                        [0-9]+          # Note: TryValidateAndNormalize adds further constrains on this.
                    )
                )?
            $",
            RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.IgnorePatternWhitespace | RegexOptions.CultureInvariant);

        private readonly string _value;

        /// <summary>
        /// Lets the object be implicitly cast to a string.
        /// </summary>
        /// <param name="input">The reference object to cast.</param>
        public static implicit operator string(RegistryReference input) => input.ToString();

        /// <summary>
        /// Checks whether the input is in the format expected of a reference to a Docker registry.
        /// </summary>
        /// <param name="input">The string to check.</param>
        /// <returns>True if the input is well-formed as a registry access; false otherwise.</returns>
        /// <exception cref="ArgumentException">The input is null or empty.</exception>
        public static bool IsValid(string input) => TryValidateAndNormalize(ref input, out _);

        /// <summary>
        /// Parses the input as a Docker registry reference.
        /// </summary>
        /// <param name="input">The string to parse.</param>
        /// <returns>An object representing the reference.</returns>
        /// <exception cref="ArgumentException">The input is null or empty.</exception>
        /// <exception cref="MalformedReferenceException">
        /// The input is not a well-formed registry access reference.
        /// </exception>
        /// <remarks>The value is automatically normalized: converted to lowercase, and port number somplified.</remarks>
        public static RegistryReference Parse(string input)
            => new(ValidateAndNormalize(input));

        /// <summary>
        /// Tries to parse the input as a Docker registry reference.
        /// </summary>
        /// <param name="input">The string to parse.</param>
        /// <param name="reference">An object representing the reference, or null if parsing failed.</param>
        /// <returns>True if the input is a well-formed registry reference; false otherwise.</returns>
        /// <exception cref="ArgumentException">The input is null or empty.</exception>
        /// <remarks>The value is automatically normalized: converted to lowercase, and port number somplified.</remarks>
        public static bool TryParse(string input, [NotNullWhen(returnValue: true)] out RegistryReference? reference)
        {
            var normalized = input;
            if (TryValidateAndNormalize(ref normalized, out _))
            {
                reference = new(normalized);
                return true;
            }
            else
            {
                reference = null;
                return false;
            }
        }

        /// <summary>
        /// Checks whether the input is in the format expected of a reference to a Docker registry, and throws an
        /// exception if not.
        /// </summary>
        /// <param name="input">The string to check.</param>
        /// <exception cref="ArgumentException">The input is null or empty.</exception>
        /// <exception cref="MalformedReferenceException">
        /// The input is not a well-formed registry access reference.
        /// </exception>
        public static void Validate(string input) => _ = ValidateAndNormalize(input);

        /// <summary>
        /// Checks whether the input is in the format expected of a reference to a Docker registry. If it is, the
        /// reference is normalized (converted to lowercase, port number simplified); otherwise it throws an exception.
        /// </summary>
        /// <param name="reference">The string to check.</param>
        /// <returns>A normalized version of the input.</returns>
        /// <exception cref="ArgumentException">The input is null or empty.</exception>
        /// <exception cref="MalformedReferenceException">
        /// The input is not a well-formed registry access reference.
        /// </exception>
        public static string ValidateAndNormalize(string reference)
        {
            if (!TryValidateAndNormalize(ref reference, out string? problem))
                throw new MalformedReferenceException(problem);
            return reference;
        }

        /// <inheritdoc/>
        public override string ToString() => _value;

        /// <summary>
        /// Checks whether the input is well-formed as a Docker registry reference. If so, it normalizes the reference;
        /// if not, it reports what's wrong about the reference.
        /// </summary>
        /// <param name="reference">
        /// The input to check. On success, this is replaced with its normalized equivalent.
        /// </param>
        /// <param name="problem">On failure, this describes what's wrong. On success, this is null.</param>
        /// <returns>True if the input is well-formed; false otherwise.</returns>
        internal static bool TryValidateAndNormalize(ref string reference, [NotNullWhen(returnValue: false)] out string? problem)
        {
            if (string.IsNullOrEmpty(reference))
                throw new ArgumentException("Null or blank values are not allowed.", nameof(reference));

            var match = _validationRegex.Match(reference);
            if (!match.Success)
            {
                problem = $"\"{reference}\" is not a well-formed registry reference.";
                return false;
            }

            // Start with the host name, converted to lowercase.
            string output = match.Groups["host"].Value.ToLowerInvariant();

            // If the port is present, normalize the number and add that.
            if (match.Groups["port"].Success)
            {
                if (!ushort.TryParse(match.Groups["port"].Value, out var port))
                {
                    problem = $"\"{reference}\" is not a well-formed registry reference: the port number is out of range.";
                    return false;
                }
                if (port == 0)
                {
                    problem = $"\"{reference}\" is not a well-formed registry reference: the port number is 0.";
                    return false;
                }
                output += ":" + port.ToString(CultureInfo.InvariantCulture);
            }

            reference = output;
            problem = null;
            return true;
        }
    }
}
