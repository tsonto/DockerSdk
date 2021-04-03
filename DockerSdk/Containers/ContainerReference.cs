using System;
using System.Diagnostics.CodeAnalysis;

namespace DockerSdk.Containers
{
    /// <summary>
    /// Represents a container name or ID.
    /// </summary>
    public abstract record ContainerReference
    {
        /// <summary>
        /// The full text of the reference.
        /// </summary>
        protected readonly string _value;

        internal ContainerReference(string value)
            => _value = value;

        /// <summary>
        /// Lets the reference be implicitly cast to a string.
        /// </summary>
        /// <param name="reference"></param>
        public static implicit operator string(ContainerReference reference) => reference.ToString();

        /// <summary>
        /// Parses the input as a Docker container name or ID.
        /// </summary>
        /// <param name="input">The text to parse.</param>
        /// <returns>The reference.</returns>
        /// <exception cref="MalformedReferenceException">
        /// The input is not a validly-formatted container reference.
        /// </exception>
        public static ContainerReference Parse(string input)
            => TryParse(input, out ContainerReference? name)
            ? name
            : throw new MalformedReferenceException($"\"{input}\" is not a valid container name or ID.");

        /// <summary>
        /// Tries to parse the input as a Docker container name or ID.
        /// </summary>
        /// <param name="input">The text to parse.</param>
        /// <param name="reference">The reference, or null if parsing failed.</param>
        /// <returns>True if parsing succeeded; false otherwise.</returns>
        /// <exception cref="ArgumentException"><paramref name="input"/> is null or empty.</exception>
        /// <remarks>
        /// Some references can be ambiguous as to whether they're IDs or names. This method will treat them as IDs. To
        /// treat them as names instead, use <see cref="ContainerName.TryParse(string, out ContainerName?)"/> and then
        /// (if that's false) <see cref="ContainerId.TryParse(string, out ContainerId?)"/>.
        /// </remarks>
        public static bool TryParse(string input, [NotNullWhen(returnValue: true)] out ContainerReference? reference)
        {
            if (ContainerId.TryParse(input, out ContainerId? id))
            {
                reference = id;
                return true;
            }
            else if (ContainerName.TryParse(input, out ContainerName? name))
            {
                reference = name;
                return true;
            }
            else
            {
                reference = null;
                return false;
            }
        }
    }
}
