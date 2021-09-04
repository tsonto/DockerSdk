using System;
using System.Collections.Generic;

namespace DockerSdk.Volumes.Dto
{
    internal class VolumeResponse
    {
        /// <summary>
        /// Date and time of when the volume was created.
        /// </summary>
        public DateTimeOffset CreatedAt { get; set; }

        /// <summary>
        /// Name of the volume driver used by the volume.
        /// </summary>
        public string Driver { get; set; } = null!;

        /// <summary>
        /// User-defined key/value metadata.
        /// </summary>
        public Dictionary<string, string> Labels { get; set; } = null!;

        /// <summary>
        /// Mount path of the volume on the host.
        /// </summary>
        public string Mountpoint { get; set; } = null!;

        /// <summary>
        /// Name of the volume.
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// The driver specific options used when creating the volume.
        /// </summary>
        public Dictionary<string, string> Options { get; set; } = null!;

        /// <summary>
        /// The level at which the volume exists. Either global for cluster-wide, or local for machine level.
        /// </summary>
        public VolumeScope Scope { get; set; }

        /// <summary>
        /// Low-level details about the volume, provided by the volume driver. This value is omitted if the volume
        /// driver does not support this feature.
        /// </summary>
        public Dictionary<string, object>? Status { get; set; }

        /// <summary>
        /// Usage details about the volume.
        /// </summary>
        public UsageData? UsageData { get; set; }
    }
}
