using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DockerSdk.JsonConverters
{
    internal class UnixEpochConverter : JsonConverter<DateTimeOffset>
    {
        public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var seconds = reader.GetInt64();
            return DateTimeOffset.FromUnixTimeSeconds(seconds);
        }

        public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
        {
            var seconds = value.ToUnixTimeSeconds();
            writer.WriteNumberValue(seconds);
        }
    }
}
