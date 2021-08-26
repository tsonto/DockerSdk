using System;
using System.Diagnostics.CodeAnalysis;

namespace DockerSdk.Volumes
{
    /// <summary>
    /// Represents the name of a Docker volume.
    /// </summary>
    public class VolumeName
    {
        /// <summary>
        /// The full text of the reference.
        /// </summary>
        protected readonly string value;

        internal VolumeName(string value)
            => this.value = value;

        /// <summary>
        /// Lets the reference be implicitly cast to a string.
        /// </summary>
        /// <param name="reference"></param>
        public static implicit operator string(VolumeName reference)
            => reference.ToString();

        /// <inheritdoc/>
        public override string ToString() => value;

        /// <summary>
        /// Parses the input as a Docker volume name.
        /// </summary>
        /// <param name="input">The text to parse.</param>
        /// <returns>The reference.</returns>
        /// <exception cref="MalformedReferenceException">
        /// The input is not a validly-formatted volume reference.
        /// </exception>
        internal static VolumeName Parse(string input)
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));
            if (TryParse(input, out var reference))
                return reference;
            throw new MalformedReferenceException($"\"{input}\" is not a valid Docker volume name.");
        }

        /// <summary>
        /// Tries to parse the input as a Docker volume name.
        /// </summary>
        /// <param name="input">The text to parse.</param>
        /// <param name="reference">The reference, or null if parsing failed.</param>
        /// <returns>True if parsing succeeded; false otherwise.</returns>
        /// <exception cref="ArgumentException"><paramref name="input"/> is null or empty.</exception>
        public static bool TryParse(string input, [NotNullWhen(returnValue: true)] out VolumeName? reference)
        {
            // TODO: are there any limits here?
            reference = new VolumeName(input);
            return true;
        }
    }
}
