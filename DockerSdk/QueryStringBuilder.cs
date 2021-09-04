using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;

namespace DockerSdk
{
    /// <summary>
    /// Builds an HTTP URL query string using Docker's conventions.
    /// </summary>
    public class QueryStringBuilder
    {
        private readonly List<string> parameters = new();

        /// <summary>
        /// Create the finalized query string. This does not include the leading '?' character.
        /// </summary>
        /// <returns>The query string.</returns>
        public string Build() => string.Join('&', parameters);

        /// <summary>
        /// Sets a given query parameter to a given value.
        /// </summary>
        /// <param name="key">The name of the parameter.</param>
        /// <param name="value">The parameter's value, or null to use the default.</param>
        /// <param name="defaultValue">The value to use if the <paramref name="value"/> argument is null.</param>
        public void Set(string key, string? value, string? defaultValue = null)
        {
            if (value == null)
                return;
            if (value == defaultValue)
                return;

            AddPair(key, value);
        }

        /// <summary>
        /// Sets a given query parameter to a given value.
        /// </summary>
        /// <param name="key">The name of the parameter.</param>
        /// <param name="value">The parameter's value, or null to use the default.</param>
        /// <param name="defaultValue">The value to use if the <paramref name="value"/> argument is null.</param>
        public void Set(string key, bool? value, bool? defaultValue = null)
        {
            if (value == null)
                return;
            if (value == defaultValue)
                return;

            AddPair(key, value.Value ? "1" : "0");
        }

        /// <summary>
        /// Sets a given query parameter to a given value.
        /// </summary>
        /// <param name="key">The name of the parameter.</param>
        /// <param name="value">The parameter's value, or null to use the default.</param>
        /// <param name="defaultValue">The value to use if the <paramref name="value"/> argument is null.</param>
        public void Set(string key, int? value, int? defaultValue = null)
        {
            if (value == null)
                return;
            if (value == defaultValue)
                return;

            AddPair(key, value.Value.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Sets a given query parameter to a given value.
        /// </summary>
        /// <param name="key">The name of the parameter.</param>
        /// <param name="value">The parameter's value, or null to use the default.</param>
        /// <param name="defaultValue">The value to use if the <paramref name="value"/> argument is null.</param>
        public void Set(string key, long? value, long? defaultValue = null)
        {
            if (value == null)
                return;
            if (value == defaultValue)
                return;

            AddPair(key, value.Value.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Sets a given query parameter to the JSON representation of the given dictionary.
        /// </summary>
        /// <param name="key">The name of the parameter.</param>
        /// <param name="values">The dictionary to serialize, or null to set the parameter to empty.</param>
        public void Set(string key, Dictionary<string, string>? values)
        {
            if (values == null || values.Count == 0)
                return;

            AddPair(key, JsonSerializer.Serialize(values));
        }

        /// <summary>
        /// Sets a given query parameter to the JSON representation of the given object.
        /// </summary>
        /// <param name="key">The name of the parameter.</param>
        /// <param name="structure">The object to serialize, or null to set the parameter to empty.</param>
        public void Set(string key, StringStringBool structure)
            => Set(key, structure.Build());

        private void AddPair(string key, string value)
        {
            parameters.Add($"{Uri.EscapeDataString(key)}={Uri.EscapeDataString(value)}");
        }

        /// <summary>
        /// Helper builder for query parameters that the Docker REST API expects in `{"option":{"value":true}}` format.
        /// </summary>
        /// <remarks>This format is common for filter parameters.</remarks>
        public class StringStringBool
        {
            private readonly Dictionary<string, Dictionary<string, bool>> structure = new();

            /// <summary>
            /// Produces the (not-URI-escaped) string representation of the structure.
            /// </summary>
            /// <returns>The equivalent value.</returns>
            public string? Build()
            {
                if (!structure.Any())
                    return null;
                return JsonSerializer.Serialize(structure);
            }

            /// <summary>
            /// Sets the given option to the given value.
            /// </summary>
            /// <param name="key">The name of the option.</param>
            /// <param name="value">The value to set.</param>
            public void Set(string key, string? value)
            {
                if (value == null)
                    return;
                structure.Add(key, new Dictionary<string, bool> { [value] = true });
            }

            /// <summary>
            /// Sets the given option to the given value.
            /// </summary>
            /// <param name="key">The name of the option.</param>
            /// <param name="value">The value to set.</param>
            public void Set(string key, int? value)
            {
                if (value == null)
                    return;
                structure.Add(key, new Dictionary<string, bool> { [value.Value.ToString(CultureInfo.InvariantCulture)] = true });
            }

            /// <summary>
            /// Sets the given option to the given values.
            /// </summary>
            /// <param name="key">The name of the option.</param>
            /// <param name="values">The values to set.</param>
            public void Set(string key, IEnumerable<string>? values)
            {
                if (values == null || !values.Any())
                    return;

                structure.Add(key, values.ToDictionary(v => v, v => true));
            }
        }
    }
}
