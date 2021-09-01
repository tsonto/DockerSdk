//using System;
//using System.Collections.Generic;
//using System.Globalization;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace DockerSdk.Core
//{
//    public class QueryParameters : List<KeyValuePair<string, string?>>
//    {
//        public QueryParameters() { }

//        public QueryParameters(IEnumerable<KeyValuePair<string, string?>> original)
//            : base(original) { }

//        public QueryParameters(IEnumerable<(string key, string? value)> original)
//            : base(original.Select(pair => new KeyValuePair<string, string?>(pair.key, pair.value))) { }

//        public void Add(string key, string value)
//            => Add(new(key, value));

//        public void Add(string key, int? value)
//        {
//            if (value == null)
//                return;
//            Add(key, value.Value);
//        }

//        public void Add(string key, int value)
//            => Add(new(key, value.ToString(CultureInfo.InvariantCulture)));

//        public void Add(string key, long? value)
//        {
//            if (value == null)
//                return;
//            Add(key, value.Value);
//        }

//        public void Add(string key, long value)
//            => Add(new(key, value.ToString(CultureInfo.InvariantCulture)));

//        public void Add(string key, bool value)
//            => Add(new(key, value ? "1" : "0"));

//        public void AddRange(IEnumerable<(string key, string? value)> pairs)
//            => AddRange(pairs.Select(pair => new KeyValuePair<string, string?>(pair.key, pair.value)));

//        //public void AddJson(string key, object? value)
//        //{
//        //    if (value == null)
//        //        return;
//        //    Add(key, System.Text.Json.JsonSerializer.Serialize(value, value.GetType()));
//        //}

//        public void AddJson<T>(string key, T? value)
//        {
//            if (value == null)
//                return;
//            Add(key, System.Text.Json.JsonSerializer.Serialize(value));
//        }

//        // TODO: implicit cast from IEnumerable<KeyValuePair<string, string?>> and IEnumerable<(string, string?)> maybe?
//    }
//}
