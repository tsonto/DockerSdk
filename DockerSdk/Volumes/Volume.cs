using System.Threading;
using System.Threading.Tasks;

namespace DockerSdk.Volumes
{
    /// <inheritdoc/>
    internal class Volume : IVolume
    {
        protected DockerClient client;

        /// <summary>
        /// Initializes a new instance of the <see cref="Volume"/> type.
        /// </summary>
        /// <param name="client">The <see cref="DockerClient"/> instance to use.</param>
        /// <param name="name">The volume's name.</param>
        internal Volume(DockerClient client, VolumeName name)
        {
            this.client = client;
            Name = name;
        }

        /// <inheritdoc/>
        public VolumeName Name { get; }

        /// <inheritdoc/>
        public Task<IVolumeInfo> GetDetailsAsync(CancellationToken ct = default)
            => VolumeFactory.LoadInfoAsync(client, Name, ct);
    }
}
