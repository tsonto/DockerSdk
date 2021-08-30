using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DockerSdk.Core
{
    public class QueryParameters : List<KeyValuePair<string, string>>
    {
        public QueryParameters() { }

        public QueryParameters(IEnumerable<KeyValuePair<string, string>> original)
            : base(original) { }

        public QueryParameters(IEnumerable<(string key, string value)> original)
            : base(original.Select(pair => new KeyValuePair<string, string>(pair.key, pair.value))) { }

        public void Add(string key, string value)
            => Add(new(key, value));

        public void AddRange(IEnumerable<(string key, string value)> pairs)
            => AddRange(pairs.Select(pair => new KeyValuePair<string, string>(pair.key, pair.value)));

        // TODO: implicit cast from IEnumerable<KeyValuePair<string, string>> and IEnumerable<(string, string)>?
    }
}
