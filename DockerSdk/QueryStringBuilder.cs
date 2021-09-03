using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;

namespace DockerSdk
{
    public class QueryStringBuilder
    {
        private readonly List<string> parameters = new();

        public string Build() => string.Join('&', parameters);

        public void Set(string key, string? value, string? defaultValue = null)
        {
            if (value == null)
                return;
            if (value == defaultValue)
                return;

            AddPair(key, value);
        }

        public void Set(string key, bool? value, bool? defaultValue = null)
        {
            if (value == null)
                return;
            if (value == defaultValue)
                return;

            AddPair(key, value.Value ? "1" : "0");
        }

        public void Set(string key, int? value, int? defaultValue = null)
        {
            if (value == null)
                return;
            if (value == defaultValue)
                return;

            AddPair(key, value.Value.ToString(CultureInfo.InvariantCulture));
        }

        public void Set(string key, long? value, long? defaultValue = null)
        {
            if (value == null)
                return;
            if (value == defaultValue)
                return;

            AddPair(key, value.Value.ToString(CultureInfo.InvariantCulture));
        }

        public void Set(string key, Dictionary<string, string>? values)
        {
            if (values == null || values.Count == 0)
                return;

            AddPair(key, JsonSerializer.Serialize(values));
        }

        public void Set(string key, StringStringBool structure)
            => Set(key, structure.Build());

        private void AddPair(string key, string value)
        {
            parameters.Add($"{Uri.EscapeDataString(key)}={Uri.EscapeDataString(value)}");
        }

        public class StringStringBool
        {
            private readonly Dictionary<string, Dictionary<string, bool>> structure = new();

            public string? Build()
            {
                if (!structure.Any())
                    return null;
                return JsonSerializer.Serialize(structure);
            }

            public void Set(string key, string? value)
            {
                if (value == null)
                    return;
                structure.Add(key, new Dictionary<string, bool> { [value] = true });
            }

            public void Set(string key, int? value)
            {
                if (value == null)
                    return;
                structure.Add(key, new Dictionary<string, bool> { [value.Value.ToString(CultureInfo.InvariantCulture)] = true });
            }

            public void Set(string key, IEnumerable<string>? values)
            {
                if (values == null || !values.Any())
                    return;

                structure.Add(key, values.ToDictionary(v => v, v => true));
            }
        }
    }
}
