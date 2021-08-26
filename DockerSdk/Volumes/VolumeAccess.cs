using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DockerSdk.Volumes
{
    /// <summary>
    /// Provides methods for interacting with Docker networks.
    /// </summary>
    public class VolumeAccess 
    {
        internal VolumeAccess(DockerClient dockerClient)
        {
            client = dockerClient;
        }

        private readonly DockerClient client;

        /// <summary>
        /// Loads an object that can be used to interact with the indicated volume.
        /// </summary>
        /// <param name="volume">A volume name.</param>
        /// <param name="ct">A token used to cancel the operation.</param>
        /// <returns>A <see cref="Task{TResult}"/> that resolves to the volume object.</returns>
        /// <exception cref="VolumeNotFoundException">No such network exists.</exception>
        /// <exception cref="System.Net.Http.HttpRequestException">
        /// The request failed due to an underlying issue such as loss of network connectivity.
        /// </exception>
        /// <exception cref="MalformedReferenceException">The volume reference is improperly formatted.</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="volume"/> input is null.</exception>
        public Task<IVolume> GetAsync(string volume, CancellationToken ct = default)
            => GetAsync(VolumeName.Parse(volume), ct);

        /// <summary>
        /// Loads an object that can be used to interact with the indicated volume.
        /// </summary>
        /// <param name="volume">A volume name.</param>
        /// <param name="ct">A token used to cancel the operation.</param>
        /// <returns>A <see cref="Task{TResult}"/> that resolves to the volume object.</returns>
        /// <exception cref="VolumeNotFoundException">No such network exists.</exception>
        /// <exception cref="System.Net.Http.HttpRequestException">
        /// The request failed due to an underlying issue such as loss of network connectivity.
        /// </exception>
        /// <exception cref="ArgumentNullException">The <paramref name="volume"/> input is null.</exception>
        public Task<IVolume> GetAsync(VolumeName volume, CancellationToken ct = default)
        {
            if (volume is null)
                throw new ArgumentNullException(nameof(volume));

            return VolumeFactory.LoadAsync(client, volume, ct);
        }

        /// <summary>
        /// Loads detailed information about a volume.
        /// </summary>
        /// <param name="volume">A volume name.</param>
        /// <param name="ct">A token used to cancel the operation.</param>
        /// <returns>A <see cref="Task{TResult}"/> that resolves to the details.</returns>
        /// <exception cref="VolumeNotFoundException">No such volume exists.</exception>
        /// <exception cref="System.Net.Http.HttpRequestException">
        /// The request failed due to an underlying issue such as network connectivity.
        /// </exception>
        /// <exception cref="MalformedReferenceException">The volume reference is improperly formatted.</exception>
        public Task<IVolumeInfo> GetDetailsAsync(string volume, CancellationToken ct = default)
            => GetDetailsAsync(VolumeName.Parse(volume), ct);

        /// <summary>
        /// Loads detailed information about a volume.
        /// </summary>
        /// <param name="volume">A volume name.</param>
        /// <param name="ct">A token used to cancel the operation.</param>
        /// <returns>A <see cref="Task{TResult}"/> that resolves to the details.</returns>
        /// <exception cref="VolumeNotFoundException">No such volume exists.</exception>
        /// <exception cref="System.Net.Http.HttpRequestException">
        /// The request failed due to an underlying issue such as network connectivity.
        /// </exception>
        public Task<IVolumeInfo> GetDetailsAsync(VolumeName volume, CancellationToken ct = default)
            => VolumeFactory.LoadInfoAsync(client, volume, ct);

        /// <summary>
        /// Gets a list of Docker volumes known to the daemon.
        /// </summary>
        /// <param name="ct">A token used to cancel the operation.</param>
        /// <returns>A <see cref="Task{TResult}"/> that resolves to the list of volumes.</returns>
        /// <remarks>The sequence of the results is undefined.</remarks>
        public async Task<IReadOnlyList<IVolume>> ListAsync(CancellationToken ct = default)
        {
            CoreModels.VolumesListResponse response;
            try
            {
                response = await client.Core.Volumes.ListAsync(ct).ConfigureAwait(false);
            }
            catch (Core.DockerApiException ex)
            {
                throw DockerException.Wrap(ex);
            }

            return response.Volumes.Select(raw => new Volume(client, new VolumeName(raw.Name))).ToArray();
        }
    }
}
