using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

namespace DockerSdk.Volumes
{
    internal static class VolumeFactory
    {
        internal static Task<IVolume> LoadAsync(DockerClient client, VolumeName name, CancellationToken ct)
        {
            if (name is null)
                throw new ArgumentNullException(nameof(name));

            IVolume volume = new Volume(client, name);
            return Task.FromResult(volume);
        }

        internal static async Task<IVolumeInfo> LoadInfoAsync(DockerClient client, VolumeName name, CancellationToken ct)
        {
            if (name is null)
                throw new ArgumentNullException(nameof(name));

            var raw = await LoadCoreAsync(client, name, ct).ConfigureAwait(false);

            return new VolumeInfo(client, name)
            {
                CreationTime = DateTimeOffset.Parse(raw.CreatedAt),
                Driver = raw.Driver,
                Labels = raw.Labels?.ToImmutableDictionary() ?? ImmutableDictionary.Create<string, string>(),
                Mountpoint = raw.Mountpoint,
                Scope = raw.Scope,
            };
        }

        private static async Task<CoreModels.VolumeResponse> LoadCoreAsync(DockerClient client, VolumeName name, CancellationToken ct)
        {
            try
            {
                return await client.Core.Volumes.InspectAsync(name, ct).ConfigureAwait(false);
            }
            catch (Core.DockerApiException ex)
            {
                if (VolumeNotFoundException.TryWrap(ex, name, out var wrapped))
                    throw wrapped;
                throw DockerException.Wrap(ex);
            }
        }
    }
}
