using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using DockerSdk.Volumes;

namespace DockerSdk.JsonConverters
{
    internal class VolumeScopeConverter : JsonConverter<VolumeScope>
    {
        public override VolumeScope Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var name = reader.GetString();
            return name switch
            {
                "local" => VolumeScope.Local,
                "global" => VolumeScope.Global,
                _ => throw new JsonException($"Unexpected volume scope \"{name}\".")
            };
        }

        public override void Write(Utf8JsonWriter writer, VolumeScope value, JsonSerializerOptions options)
        {
            var name = value switch
            {
                VolumeScope.Global => "global",
                VolumeScope.Local => "local",
                _ => throw new JsonException($"Unexpected volume scope \"{value}\".")
            };
            writer.WriteStringValue(name);
        }
    }
}
