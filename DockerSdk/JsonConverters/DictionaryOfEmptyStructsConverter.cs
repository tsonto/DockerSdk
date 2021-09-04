using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DockerSdk.JsonConverters
{
    internal class DictionaryOfEmptyStructsConverter : JsonConverter<IList<string>>
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
