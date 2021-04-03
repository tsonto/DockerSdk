﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace DockerSdk.Containers
{
    /// <summary>
    /// Represents a Docker container's full-length ID.
    /// </summary>
    public record ContainerFullId : ContainerId
    {
        internal ContainerFullId(string id) : base(id) { }

        /// <inheritdoc/>
        public override string ToString() => _value;

        /// <summary>
        /// Gets the short form of the ID.
        /// </summary>
        public ContainerId ShortForm => new(Shorten(_value));

        /// <summary>
        /// Tries to parse the input as a Docker container full ID.
        /// </summary>
        /// <param name="input">The string to parse.</param>
        /// <param name="id">The ID, or null if parsing failed.</param>
        /// <returns>True if the input is well-formed as a container ID; false otherwise.</returns>
        public static bool TryParse(string input, [NotNullWhen(returnValue: true)] out ContainerFullId? id)
        {
            if (string.IsNullOrEmpty(input))
                throw new ArgumentException($"'{nameof(input)}' cannot be null or empty.", nameof(input));

            if (_fullIdRegex.IsMatch(input))
            {
                id = new(input);
                return true;
            }
            else
            {
                id = null;
                return false;
            }
        }

        private static readonly Regex _fullIdRegex = new(
            @"^
                [a-f0-9]{64}    # Exactly 64 lowercase hex digits
            $",
            RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.IgnorePatternWhitespace);
    }
}
