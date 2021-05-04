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
    public class ImageDetails : Image
    {
        internal ImageDetails(DockerClient client, CoreModels.ImageInspectResponse raw, ImageDetails? parent)
            : base(client, new ImageFullId(raw.ID))
        {
            Author = raw.Author ?? "";
            Comment = raw.Comment ?? "";
            CreationTime = raw.Created;
            Digest = raw.RepoDigests?.FirstOrDefault();
            Labels = raw.Config?.Labels?.ToImmutableDictionary() ?? ImmutableDictionary.Create<string, string>();
            OsType = Enum.Parse<GuestOsType>(raw.Os, true);
            ParentImage = parent;
            Size = raw.Size;
            Tags = raw.RepoTags?.Select(ImageName.Parse)?.ToImmutableArray() ?? ImmutableArray<ImageName>.Empty;
            VirtualSize = raw.VirtualSize;
            WorkingDirectory = raw.Config?.WorkingDir ?? GetDefaultWorkingDirectory(OsType);

            // Note: Config is the image's configuration; ContainerConfig is the configuration of the container from
            //       which the image was built, which we generally don't care about.
        }

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
        /// Gets the labels that have been applied to the image.
        /// </summary>
        public IReadOnlyDictionary<string, string> Labels { get; }

        /// <summary>
        /// Gets the kind of OS that the image runs.
        /// </summary>
        /// <remarks>
        /// The only situation where this can be Windows is when running Docker for Windows (which only runs on Windows
        /// hosts) in Windows containers mode. In that situation Linux containers will not be visible.
        /// </remarks>
        public GuestOsType OsType { get; }

        /// <summary>
        /// Gets the image that this image was built from, of any.
        /// </summary>
        /// <remarks>
        /// Due to the way that Docker works, this will usually be null:
        /// <list type="bullet">
        /// <item>Images using image manifest v2 (the default since Docker 1.3.0) that are pulled from a registry will never have a parent image set.</item>
        /// <item>Images built with Buildkit (the default since Docker 20.10) seem to never set the parent.</item>
        /// </list>
        /// When the parent image is null, the history will not have any image/layer identifiers either, except the current image's.
        /// </remarks>
        public ImageDetails? ParentImage { get; }

        /// <summary>
        /// Gets the size, in bytes, of the image's writable layer.
        /// </summary>
        /// <remarks>
        /// Mounting the first container for the image will consume memory equal to the virtual size plus the size. Each
        /// subsequent container will consume memory equal to the size. These sizes do not include space consumed for
        /// log files, volumes, configuration files, swap space, or checkpoints.
        /// </remarks>
        public long Size { get; }

        /// <summary>
        /// Gets the names that the image is known by.
        /// </summary>
        public IReadOnlyList<ImageName> Tags { get; }

        /// <summary>
        /// Gets the size, in bytes, of the image's read-only layers.
        /// </summary>
        /// <remarks>
        /// Mounting the first container for the image will consume memory equal to the virtual size plus the size. Each
        /// subsequent container will consume memory equal to the size. These sizes do not include space consumed for
        /// log files, volumes, configuration files, swap space, or checkpoints.
        /// </remarks>
        public long VirtualSize { get; }

        /// <summary>
        /// Gets the image's initial working directory. If the image did not specify a working directory, this will be
        /// set to the filesystem's root path.
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
        internal static async Task<ImageDetails> LoadAsync(DockerClient client, ImageReference image, CancellationToken ct = default)
        {
            if (image is null)
                throw new ArgumentNullException(nameof(image));

            // Fetch the main portion of the image's details.
            CoreModels.ImageInspectResponse response;
            try
            {
                response = await client.Core.Images.InspectImageAsync(image, ct).ConfigureAwait(false);
            }
            catch (Core.DockerApiException ex)
            {
                if (ImageNotFoundLocallyException.TryWrap(ex, image, out var wrapped))
                    throw wrapped;
                throw DockerException.Wrap(ex);
            }

            // If the image's parent is known, recursively fetch its details.
            ImageDetails? parent = null;
            if (!string.IsNullOrEmpty(response.Parent))
                parent = await LoadAsync(client, new ImageId(response.Parent), ct);

            return new ImageDetails(response, parent);
        }

        private static string GetDefaultWorkingDirectory(GuestOsType os)
            => os switch
            {
                GuestOsType.Windows => @"C:\",
                _ => "/"
            };

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
