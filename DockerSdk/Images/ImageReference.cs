using System;
using System.Diagnostics.CodeAnalysis;

namespace DockerSdk.Images
{
    /// <summary>
    /// Represents an image name or ID.
    /// </summary>
    public abstract record ImageReference
    {
        /// <summary>
        /// Tries to parse the input as a Docker image name or ID.
        /// </summary>
        /// <param name="input">The text to parse.</param>
        /// <param name="reference">The reference, or null if parsing failed.</param>
        /// <returns>True if parsing succeeded; false otherwise.</returns>
        /// <exception cref="ArgumentException"><paramref name="input"/> is null or empty.</exception>
        public static bool TryParse(string input, [NotNullWhen(returnValue: true)] out ImageReference? reference)
        {
            if (!ImageReferenceParser.TryParse(input, out DecomposedImageReference? parsed))
            {
                reference = null;
                return false;
            }
            else
            {
                reference = parsed.ToReference();
                return true;
            }
        }

        /// <summary>
        /// Parses the input as a Docker image name or ID.
        /// </summary>
        /// <param name="input">The text to parse.</param>
        /// <returns>The reference.</returns>
        /// <exception cref="MalformedReferenceException">
        /// The input is not a validly-formatted image reference.
        /// </exception>
        public static ImageReference Parse(string input)
            => TryParse(input, out ImageReference? name)
            ? name
            : throw new MalformedReferenceException($"\"{input}\" is not a valid image name or ID.");

        /// <summary>
        /// The full text of the reference.
        /// </summary>
        protected readonly string _value;

        internal ImageReference(string value)
        {
            if (string.IsNullOrEmpty(value))
                throw new System.ArgumentException($"'{nameof(value)}' cannot be null or empty.", nameof(value));

            _value = value;
        }

        /// <inheritdoc/>
        public override string ToString() => _value;

        /// <summary>
        /// Lets the reference be implicitly cast to a string.
        /// </summary>
        /// <param name="reference"></param>
        public static implicit operator string(ImageReference reference) => reference._value;
    }
}
