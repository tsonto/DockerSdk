using System.Collections.Generic;
using System.Collections.Immutable;

namespace DockerSdk
{
    internal static class ExtensionsForEnumerables
    {
        public static Dictionary<K, V> ToDictionary<K, V>(this IEnumerable<KeyValuePair<K, V>> kvps)
            where K : notnull
            => new Dictionary<K, V>(kvps);

        public static ImmutableDictionary<K, V> ToImmutableDictionary<K, V>(this IEnumerable<KeyValuePair<K, V>> kvps)
            where K : notnull
            => ImmutableDictionary<K, V>.Empty.AddRange(kvps);
    }
}
