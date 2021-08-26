using System.Threading;
using System.Threading.Tasks;

namespace DockerSdk.Volumes
{
    internal class Volume : IVolume
    {
        protected DockerClient client;

        public Volume(DockerClient client, VolumeName name)
        {
            this.client = client;
            Name = name;
        }

        public VolumeName Name { get; }

        public Task<IVolumeInfo> GetDetailsAsync(CancellationToken ct = default)
            => VolumeFactory.LoadInfoAsync(client, Name, ct);
    }
}
