using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DockerSdk.Volumes
{
    public interface IVolume
    {
        VolumeName Name { get; }

        Task<IVolumeInfo> GetDetailsAsync(CancellationToken ct = default);
    }

    public interface IVolumeInfo : IVolume
    {
        DateTimeOffset CreationTime { get; }

        string Driver { get; }

        /// <summary>
        /// Gets the labels that have been applied to the volume.
        /// </summary>
        IReadOnlyDictionary<string, string> Labels { get; }

        string Mountpoint { get; }

        VolumeScope Scope { get; }

        // TODO: Options
    }
}
