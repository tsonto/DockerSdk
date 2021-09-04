using System.Text.Json.Serialization;
using DockerSdk.JsonConverters;

namespace DockerSdk.Volumes
{
    /// <summary>
    /// Indicates the level at which a volume exists.
    /// </summary>
    [JsonConverter(typeof(VolumeScopeConverter))]
    public enum VolumeScope
    {
        /// <summary>
        /// The volume exists on the local machine only.
        /// </summary>
        Local,

        /// <summary>
        /// The volume exists at the cluster level.
        /// </summary>
        Global,
    }
}
