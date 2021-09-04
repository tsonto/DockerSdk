using System;
using System.Collections.Generic;

namespace DockerSdk.Volumes
{
    /// <summary>
    /// Provides detailed information about a volume.
    /// </summary>
    /// <remarks>This class holds a snapshot in time. Its information is immutable once created.</remarks>
    public interface IVolumeInfo : IVolume
    {
        /// <summary>
        /// Gets the time at which the volume was created.
        /// </summary>
        DateTimeOffset CreationTime { get; }

        /// <summary>
        /// Gets the name of the storage driver that operates the volume.
        /// </summary>
        string Driver { get; }

        /// <summary>
        /// Gets the labels that have been applied to the volume.
        /// </summary>
        IReadOnlyDictionary<string, string> Labels { get; }

        /// <summary>
        /// Gets the absolute path of where the volume's data is stored on the host.
        /// </summary>
        string Mountpoint { get; }

        /// <summary>
        /// Gets the level at which the volume exists: either Global for cluster-wide or Local for machine-level.
        /// </summary>
        VolumeScope Scope { get; }

        // TODO: Options
    }
}
