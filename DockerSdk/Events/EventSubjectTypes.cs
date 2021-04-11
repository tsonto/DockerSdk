using System;

namespace DockerSdk.Events
{
    /// <summary>
    /// Encspsulates functionality about <see cref="EventSubjectType"/> that C# doesn't let us put directly on that
    /// type.
    /// </summary>
    internal static class EventSubjectTypes
    {
        /// <summary>
        /// Parses a string to a <see cref="EventSubjectType"/>.
        /// </summary>
        /// <param name="input">The string to parse.</param>
        /// <returns>The resultant enum value.</returns>
        public static EventSubjectType Parse(string input)
            => Enum.Parse<EventSubjectType>(input, ignoreCase: true);
    }
}
