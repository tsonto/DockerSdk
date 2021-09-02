using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using DockerSdk.Containers.Dto;

namespace DockerSdk.JsonConverters
{
    internal class RestartPolicyKindConverter : JsonConverter<RestartPolicyKind>
    {
        public override RestartPolicyKind Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var name = reader.GetString();
            return name switch
            {
                null => RestartPolicyKind.Undefined,
                "no" => RestartPolicyKind.No,
                "always" => RestartPolicyKind.Always,
                "on-failure" => RestartPolicyKind.OnFailure,
                "unless-stopped" => RestartPolicyKind.UnlessStopped,
                _ => throw new JsonException($"Unexpected restart policy \"{name}\".")
            };
        }

        public override void Write(Utf8JsonWriter writer, RestartPolicyKind value, JsonSerializerOptions options)
        {
            var name = value switch
            {
                RestartPolicyKind.Undefined => null,
                RestartPolicyKind.No => "no",
                RestartPolicyKind.Always => "always",
                RestartPolicyKind.OnFailure => "on-failure",
                RestartPolicyKind.UnlessStopped => "unless-stopped",
                _ => throw new JsonException($"Unexpected restart policy \"{value}\".")
            };
            writer.WriteStringValue(name);
        }
    }
}
