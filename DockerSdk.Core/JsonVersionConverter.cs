using System;
using Newtonsoft.Json;

namespace DockerSdk.Core
{
    internal class JsonVersionConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object? value, Newtonsoft.Json.JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, Newtonsoft.Json.JsonSerializer serializer)
        {
            if (reader.Value is string strVal)
                return Version.Parse(strVal);
            
            var valueType = reader.Value == null ? "<null>" : reader.Value.GetType().FullName;
            throw new InvalidOperationException($"Cannot deserialize value of type '{valueType}' to '{objectType.FullName}'.");
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof (Version);
        }
    }
}
