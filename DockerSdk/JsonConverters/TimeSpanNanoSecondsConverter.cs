using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DockerSdk.JsonConverters
{
    internal class TimeSpanNanosecondsConverter : JsonConverter<TimeSpan>
    {
        private const int NanosecondsPerMillisecond = 1_000_000;

        public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var nanoseconds = reader.GetInt64();
            return TimeSpan.FromMilliseconds(nanoseconds / NanosecondsPerMillisecond);
        }

        public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
        {
            var nanoseconds = value.TotalMilliseconds * NanosecondsPerMillisecond;
            writer.WriteNumberValue((long)nanoseconds);
        }
    }
}
