using System.Text.Json.Serialization;
using DockerSdk.JsonConverters;

namespace DockerSdk.Volumes
{
    [JsonConverter(typeof(VolumeScopeConverter))]
    public enum VolumeScope
    {
        Local,
        Global,
    }
}
