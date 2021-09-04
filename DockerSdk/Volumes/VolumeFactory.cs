using System;
using System.Collections.Immutable;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DockerSdk.Volumes.Dto;

namespace DockerSdk.Volumes
{
    internal static class VolumeFactory
    {
        internal static Task<IVolume> LoadAsync(DockerClient client, VolumeName name, CancellationToken _)
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
                CreationTime = raw.CreatedAt,
                Driver = raw.Driver,
                Labels = raw.Labels?.ToImmutableDictionary() ?? ImmutableDictionary.Create<string, string>(),
                Mountpoint = raw.Mountpoint,
                Scope = raw.Scope,
            };
        }

        private static Task<VolumeResponse> LoadCoreAsync(DockerClient client, VolumeName name, CancellationToken ct)
        {
            return client.BuildRequest(HttpMethod.Get, $"volumes/{name}")
                .RejectStatus(HttpStatusCode.NotFound, _ => new VolumeNotFoundException($"No volume with name \"{name}\" exists."))
                .SendAsync<VolumeResponse>(ct);
        }
    }
}
