using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DockerSdk.Core.JsonConverters
{
    internal class DictionaryOfEmptyStructsConverter : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
        {
            if (typeToConvert == typeof(IList<string>))
                return true;

            return false;
        }

        public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            if (typeToConvert == typeof(IList<string>))
                return new ListConverter();

            throw new InvalidOperationException();
        }

        //private JsonConverter? Create(Type genericType, JsonSerializerOptions options, params Type[] typeArgs)
        //    => (JsonConverter?)Activator.CreateInstance(
        //        genericType.MakeGenericType(typeArgs),
        //        BindingFlags.Instance | BindingFlags.Public,
        //        binder: null,
        //        args: new object[] { options },
        //        culture: null);

        private class ListConverter : JsonConverter<IList<string>>
        {
            public override IList<string>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType == JsonTokenType.Null)
                    return null;
                if (reader.TokenType != JsonTokenType.StartObject)
                    throw new JsonException();

                var output = new List<string>();

                while (reader.Read() && reader.TokenType == JsonTokenType.PropertyName)
                {
                    // Add the key to the list.
                    output.Add(reader.GetString()!);

                    // Skip the next two tokens, which represent an empty object.
                    reader.Read();
                    reader.Read();
                }

                if (reader.TokenType != JsonTokenType.EndObject)
                    throw new JsonException();

                return output;
            }

            public override void Write(Utf8JsonWriter writer, IList<string> value, JsonSerializerOptions options)
            {
                writer.WriteStartObject();
                foreach (var element in value)
                {
                    writer.WritePropertyName(element);
                    writer.WriteStartObject();
                    writer.WriteEndObject();
                }
                writer.WriteEndObject();
            }
        }
    }
}
