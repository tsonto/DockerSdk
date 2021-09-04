using System.Threading;
using System.Threading.Tasks;

namespace DockerSdk.Volumes
{
    /// <summary>
    /// Represents a Docker volume, which is a persistent filesystem mount used by containers.
    /// </summary>
    /// <seealso cref="IVolumeInfo"/>
    public interface IVolume
    {
        /// <summary>
        /// Gets the volume's name.
        /// </summary>
        VolumeName Name { get; }

        /// <summary>
        /// Gets detailed information about the volume.
        /// </summary>
        /// <param name="ct">A <see cref="CancellationToken"/> used to cancel the operation.</param>
        /// <returns>A <see cref="Task"/> that completes when the result is available.</returns>
        /// <exception cref="VolumeNotFoundException">The volume no longer exists.</exception>
        /// <exception cref="System.Net.Http.HttpRequestException">
        /// The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate
        /// validation, or timeout.
        /// </exception>
        Task<IVolumeInfo> GetDetailsAsync(CancellationToken ct = default);

    }
}
