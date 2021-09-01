using System;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DockerSdk.Images.Dto;

namespace DockerSdk.Images
{
    internal static class ImageFactory
    {
        internal static async Task<IImage> LoadAsync(DockerClient client, ImageReference image, CancellationToken ct = default)
        {
            if (image is null)
                throw new ArgumentNullException(nameof(image));

            var raw = await LoadCoreAsync(client, image, ct).ConfigureAwait(false);

            var id = new ImageFullId(raw.Id);
            return new Image(client, id);
        }

        /// <summary>
        /// Creates an instance of <see cref="ImageInfo"/> based on information it loads from the Docker daemon.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="image"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        /// <exception cref="ImageNotFoundException">No such image exists.</exception>
        /// <exception cref="System.Net.Http.HttpRequestException">
        /// The request failed due to an underlying issue such as loss of network connectivity.
        /// </exception>
        internal static async Task<IImageInfo> LoadInfoAsync(DockerClient client, ImageReference image, CancellationToken ct = default)
        {
            if (image is null)
                throw new ArgumentNullException(nameof(image));

            var raw = await LoadCoreAsync(client, image, ct).ConfigureAwait(false);

            var id = new ImageFullId(raw.Id);
            var osType = Enum.Parse<GuestOsType>(raw.OS, true);
            var parent = string.IsNullOrEmpty(raw.Parent)
                ? null
                : new Image(client, new ImageFullId(raw.Parent));

            return new ImageInfo(client, id)
            {
                Author = raw.Author ?? "",
                Comment = raw.Comment ?? "",
                CreationTime = raw.Created,
                Digest = raw.RepoDigests?.FirstOrDefault(),
                Labels = raw.Config?.Labels?.ToImmutableDictionary() ?? ImmutableDictionary.Create<string, string>(),
                OsType = osType,
                ParentImage = parent,
                Size = raw.Size,
                Tags = raw.RepoTags?.Select(ImageName.Parse)?.ToImmutableArray() ?? ImmutableArray<ImageName>.Empty,
                VirtualSize = raw.VirtualSize,
                WorkingDirectory = raw.Config?.WorkingDir ?? GetDefaultWorkingDirectory(osType),
            };

            // Note: Config is the image's configuration; ContainerConfig is the configuration of the container from
            // which the image was built, which we generally don't care about.
        }

        private static string GetDefaultWorkingDirectory(GuestOsType os)
            => os switch
            {
                GuestOsType.Windows => @"C:\",
                _ => "/"
            };

        private static Task<ImageInspectResponse> LoadCoreAsync(DockerClient client, ImageReference image, CancellationToken ct)
            => client.BuildRequest(HttpMethod.Get, $"images/{image}/json")
                .RejectStatus(HttpStatusCode.NotFound, _ => new ImageNotFoundLocallyException($"Image {image} does not exist locally."))
                .AcceptStatus(HttpStatusCode.OK)
                .SendAsync<ImageInspectResponse>(ct);
    }
}
