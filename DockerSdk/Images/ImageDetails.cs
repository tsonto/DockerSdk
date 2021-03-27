using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Core = Docker.DotNet;
using CoreModels = Docker.DotNet.Models;

namespace DockerSdk.Images
{
    /// <remarks>This class holds a snapshot in time. Its information is immutable once created.</remarks>
    public class ImageDetails
    {
        internal ImageDetails(CoreModels.ImageInspectResponse raw, ImageDetails? parent)
        {
            Author = raw.Author ?? "";
            Comment = raw.Comment ?? "";
            CreationTime = raw.Created;
            Digest = raw.RepoDigests?.FirstOrDefault();
            Id = raw.ID;
            IdShort = ImageAccess.ShortenId(raw.ID);
            Labels = raw.ContainerConfig?.Labels?.ToImmutableDictionary() ?? ImmutableDictionary<string,string>.Empty;
            Os = raw.Os;
            ParentImage = parent;
            Size = raw.Size;
            Tags = raw.RepoTags?.ToImmutableArray() ?? ImmutableArray<string>.Empty;
            VirtualSize = raw.VirtualSize;
            WorkingDirectory = raw.ContainerConfig?.WorkingDir ?? GetDefaultWorkingDirectory(Os);
        }

        private static string? GetDefaultWorkingDirectory(string os)
            => os switch
            {
                "windows" => @"C:\",
                _ => "/"
            };

        /// <summary>
        /// Gets the image's author, or a blank string if no author information is available.
        /// </summary>
        public string Author { get; }

        /// <summary>
        /// Gets the comment saved with the image, or a blank string if no comment is available.
        /// </summary>
        public string Comment { get; }

        /// <summary>
        /// Gets the time at which the image was created.
        /// </summary>
        public DateTimeOffset CreationTime { get; }

        /// <summary>
        /// Gets the image's digest.
        /// </summary>
        /// <remarks>
        /// This is a hash of the image's manifest, which is effectively a list of the layers used to produce the image.
        /// It represents the image's build inputs--that is, if the build is changed, the digest will change too. Note
        /// that this value is not calculated until the image is pushed/pulled.
        /// </remarks>
        public string? Digest { get; }

        /// <summary>
        /// Gets the image's full ID, including the hash function prefix.
        /// </summary>
        /// <remarks>
        /// This is a hash of the image's config file. Even with identical build inputs, this will be different for each
        /// build.
        /// </remarks>
        public string Id { get; }

        /// <summary>
        /// Gets the short form of the ID, which is 12 hex digits long and does not include the hash function prefix.
        /// </summary>
        public string IdShort { get; }

        /// <summary>
        /// Gets the labels that have been applied to the image.
        /// </summary>
        public IReadOnlyDictionary<string,string> Labels { get; }
        public string Os { get; }

        /// <summary>
        /// Gets the image that this image was built from, of any.
        /// </summary>
        /// <remarks>
        /// Due to the way Docker works, this will always be <see langword="null"/> for images pulled from a registry
        /// (as opposed to images built locally).
        /// </remarks>
        public ImageDetails? ParentImage { get; }

        public long Size { get; }

        /// <summary>
        /// Gets the names that the image is known by.
        /// </summary>
        public IReadOnlyList<string> Tags { get; }

        public long VirtualSize { get; }

        /// <summary>
        /// Gets the image's initial working directory. If the image did not specify a working directory, this will be set to the filesystem's root path.
        /// </summary>
        /// <remarks>This directory is guaranteed to exist inside the image.</remarks>
        public string WorkingDirectory { get; }

        /// <summary>
        /// Creates an instance of <see cref="ImageDetails"/> based on information it loads from the Docker daemon.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="image"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        /// <exception cref="ImageNotFoundException">No such image exists.</exception>
        /// <exception cref="System.Net.Http.HttpRequestException">
        /// The request failed due to an underlying issue such as loss of network connectivity.
        /// </exception>
        internal static async Task<ImageDetails> LoadAsync(DockerClient client, string image, CancellationToken ct = default)
        {
            // Scrub the input.
            if (string.IsNullOrEmpty(image))
                throw new ArgumentException($"'{nameof(image)}' cannot be null or empty", nameof(image));

            // Fetch the main portion of the image's details.
            CoreModels.ImageInspectResponse response;
            try
            {
                response = await client.Core.Images.InspectImageAsync(image, ct).ConfigureAwait(false);
            }
            catch (Core.DockerImageNotFoundException ex)
            {
                throw ImageNotFoundException.Wrap(image, ex);
            }
            catch (Core.DockerApiException ex)
            {
                throw DockerException.Wrap(ex);
            }

            // If the image's parent is known, recursively fetch its details.
            ImageDetails? parent = null;
            if (!string.IsNullOrEmpty(response.Parent))
                parent = await LoadAsync(client, response.Parent, ct);

            return new ImageDetails(response, parent);
        }

        // TODO: Container and ContainerConfig -- these represent the temporary container created when building the image
        // TODO: DockerVersion -- the version of Docker that created the image
        // TODO: Config - This is what the ID is based on
        // TODO: Architecture
        // TODO: Variant
        // TODO: Os
        // TODO: OsVersion
        // TODO: GraphicDriver
        // TODO: RootFS
        // TODO: ImageMetadata
    }
}
