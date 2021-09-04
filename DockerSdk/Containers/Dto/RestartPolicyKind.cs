using System.Text.Json.Serialization;
using DockerSdk.JsonConverters;

namespace DockerSdk.Containers.Dto
{
    [JsonConverter(typeof(RestartPolicyKindConverter))]
    internal enum RestartPolicyKind
    {
        Undefined,
        No,
        Always,
        OnFailure,
        UnlessStopped
    }
}
