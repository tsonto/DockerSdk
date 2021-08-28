using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DockerSdk.Core
{
    internal class JsonBase64Converter : JsonConverter
    {
        private static readonly Type _byteListType = typeof(IList<byte>);
        public override bool CanRead => true;

        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer, object? value, Newtonsoft.Json.JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, Newtonsoft.Json.JsonSerializer serializer)
        {
            if (reader.Value is not string strVal)
                return null;

            return Convert.FromBase64String(strVal);
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == _byteListType;
        }
    }
}
