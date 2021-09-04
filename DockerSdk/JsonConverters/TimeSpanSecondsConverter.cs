using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DockerSdk.JsonConverters
{
    internal class TimeSpanSecondsConverter : JsonConverter<TimeSpan>
    {
        public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var seconds = reader.GetInt64();
            return TimeSpan.FromSeconds(seconds);
        }

        public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value.TotalSeconds);
        }
    }
}
