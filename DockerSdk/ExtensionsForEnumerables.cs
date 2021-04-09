using System.Collections.Generic;

namespace DockerSdk
{
    internal static class ExtensionsForEnumerables
    {
        public static Dictionary<K, V> ToDictionary<K, V>(this IEnumerable<KeyValuePair<K, V>> kvps)
            where K : notnull
            => new Dictionary<K, V>(kvps);
    }
}
