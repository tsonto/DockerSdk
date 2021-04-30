using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace DockerSdk
{
    internal static class ExtensionsForEnumerables
    {
        public static IDictionary<K, VNew> SelectValues<K, VOld, VNew>(this IDictionary<K, VOld> input, Func<VOld, VNew> transform)
            where K : notnull
            => input
                .Select(kvp => new KeyValuePair<K, VNew>(kvp.Key, transform(kvp.Value)))
                .ToDictionary();

        public static Dictionary<K, V> ToDictionary<K, V>(this IEnumerable<KeyValuePair<K, V>> kvps)
                    where K : notnull
            => new Dictionary<K, V>(kvps);

        public static ImmutableDictionary<K, V> ToImmutableDictionary<K, V>(this IEnumerable<KeyValuePair<K, V>> kvps)
            where K : notnull
            => ImmutableDictionary<K, V>.Empty.AddRange(kvps);
    }
}
