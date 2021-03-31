using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Core = Docker.DotNet;
using CoreModels = Docker.DotNet.Models;

namespace DockerSdk.Images
{
    /// <summary>
    /// Provides methods for interacting with Docker images.
    /// </summary>
    public class ImageAccess
    {
        internal ImageAccess(DockerClient docker)
        {
            _docker = docker;
        }

        private readonly DockerClient _docker;

        // TODO: ExportAsync (docker image save)

        /// <summary>
        /// Loads an object that can be used to interact with the indicated image.
        /// </summary>
        /// <param name="image">An image name or ID.</param>
        /// <param name="ct">A token used to cancel the operation.</param>
        /// <returns>A <see cref="Task{TResult}"/> that resolves to the image object.</returns>
        /// <exception cref="ImageNotFoundException">No such image exists.</exception>
        /// <exception cref="System.Net.Http.HttpRequestException">
        /// The request failed due to an underlying issue such as loss of network connectivity.
        /// </exception>
        /// <exception cref="MalformedReferenceException">The image reference is improperly formatted.</exception>
        public Task<Image> GetAsync(string image, CancellationToken ct = default)
            => GetAsync(ImageReference.Parse(image), ct);

        /// <summary>
        /// Loads an object that can be used to interact with the indicated image.
        /// </summary>
        /// <param name="image">An image name or ID.</param>
        /// <param name="ct">A token used to cancel the operation.</param>
        /// <returns>A <see cref="Task{TResult}"/> that resolves to the image object.</returns>
        /// <exception cref="ImageNotFoundException">No such image exists.</exception>
        /// <exception cref="System.Net.Http.HttpRequestException">
        /// The request failed due to an underlying issue such as loss of network connectivity.
        /// </exception>
        public async Task<Image> GetAsync(ImageReference image, CancellationToken ct = default)
        {
            try
            {
                CoreModels.ImageInspectResponse response = await _docker.Core.Images.InspectImageAsync(image, ct).ConfigureAwait(false);
                return new Image(_docker, new ImageFullId(response.ID));
            }
            catch (Core.DockerImageNotFoundException ex)
            {
                throw ImageNotFoundException.Wrap(image, ex);
            }
            catch (Core.DockerApiException ex)
            {
                throw DockerException.Wrap(ex);
            }
        }

        /// <summary>
        /// Loads detailed information about an image.
        /// </summary>
        /// <param name="image">An image name or ID.</param>
        /// <param name="ct">A token used to cancel the operation.</param>
        /// <returns>A <see cref="Task{TResult}"/> that resolves to the details.</returns>
        /// <exception cref="ImageNotFoundException">No such image exists.</exception>
        /// <exception cref="System.Net.Http.HttpRequestException">
        /// The request failed due to an underlying issue such as network connectivity.
        /// </exception>
        /// <exception cref="MalformedReferenceException">The image reference is improperly formatted.</exception>
        public Task<ImageDetails> GetDetailsAsync(string image, CancellationToken ct = default)
            => ImageDetails.LoadAsync(_docker, ImageReference.Parse(image), ct);

        /// <summary>
        /// Loads detailed information about an image.
        /// </summary>
        /// <param name="image">An image name or ID.</param>
        /// <param name="ct">A token used to cancel the operation.</param>
        /// <returns>A <see cref="Task{TResult}"/> that resolves to the details.</returns>
        /// <exception cref="ImageNotFoundException">No such image exists.</exception>
        /// <exception cref="System.Net.Http.HttpRequestException">
        /// The request failed due to an underlying issue such as network connectivity.
        /// </exception>
        public Task<ImageDetails> GetDetailsAsync(ImageReference image, CancellationToken ct = default)
            => ImageDetails.LoadAsync(_docker, image, ct);

        // TODO: GetHistoryAsync (docker image history)

        // TODO: ImportAsync

        /// <summary>
        /// Gets a list of Docker images known to the daemon.
        /// </summary>
        /// <param name="ct">A token used to cancel the operation.</param>
        /// <returns>A <see cref="Task{TResult}"/> that resolves to the list of images.</returns>
        /// <remarks>
        /// <para>
        /// This method does not necessarily return the same results as the `docker image ls` command. To get the same
        /// results, use the <see cref="ListAsync(ListImagesOptions, CancellationToken)"/> overload and give it the <see
        /// cref="ListImagesOptions.CommandLineDefaults"/> options.
        /// </para>
        /// <para>The sequence of the results is undefined.</para>
        /// </remarks>
        public Task<IReadOnlyList<Image>> ListAsync(CancellationToken ct = default)
            => ListAsync(new(), ct);

        /// <summary>
        /// Gets a list of Docker images known to the daemon.
        /// </summary>
        /// <param name="options">Filters for the search.</param>
        /// <param name="ct">A token used to cancel the operation.</param>
        /// <returns>A <see cref="Task{TResult}"/> that resolves to the list of images.</returns>
        /// <remarks>
        /// <para>
        /// This method does not necessarily return the same results as the `docker image ls` command by default. To get
        /// the same results, pass in <see cref="ListImagesOptions.CommandLineDefaults"/>.
        /// </para>
        /// <para>The sequence of the results is undefined.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="options"/> is null.</exception>
        public async Task<IReadOnlyList<Image>> ListAsync(ListImagesOptions options, CancellationToken ct = default)
        {
            if (options is null)
                throw new ArgumentNullException(nameof(options));

            var request = new CoreModels.ImagesListParameters
            {
                All = !options.HideIntermediateImages,
                Filters = MakeListFilters(options),
            };

            IList<CoreModels.ImagesListResponse> response;
            try
            {
                response = await _docker.Core.Images.ListImagesAsync(request, ct).ConfigureAwait(false);
            }
            catch (Core.DockerApiException ex)
            {
                throw DockerException.Wrap(ex);
            }

            return response.Select(raw => new Image(_docker, new ImageFullId(raw.ID))).ToArray();
        }

        /// <summary>
        /// Retrieves the indicated image from its remote home.
        /// </summary>
        /// <param name="image">A reference to the image to fetch.</param>
        /// <param name="ct">A token used to cancel the operation.</param>
        /// <returns>A <see cref="Task{TResult}"/> that resolves to the downloaded image.</returns>
        /// <remarks>
        /// This instructs the Docker daemon to download the image from the image's home registry. The registry is
        /// determined based on the image's name, defaulting to "docker.io".
        /// </remarks>
        /// <exception cref="ArgumentException"><paramref name="image"/> is null or empty.</exception>
        /// <exception cref="MalformedReferenceException">The input could not be parsed as an image name.</exception>
        /// <exception cref="InvalidOperationException">
        /// One Task removed the auth object while another was getting it.
        /// </exception>
        /// <exception cref="RegistryAuthException">
        /// The registry requires credentials that the client hasn't been given.
        /// </exception>
        /// <exception cref="System.Net.Http.HttpRequestException">
        /// The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate
        /// validation, or timeout.
        /// </exception>
        public Task<Image> PullAsync(string image, CancellationToken ct = default)
            => PullAsync(ImageReference.Parse(image), ct);

        /// <summary>
        /// Retrieves the indicated image from its remote home.
        /// </summary>
        /// <param name="image">A reference to the image to fetch.</param>
        /// <param name="ct">A token used to cancel the operation.</param>
        /// <returns>A <see cref="Task{TResult}"/> that resolves to the downloaded image.</returns>
        /// <remarks>
        /// This instructs the Docker daemon to download the image from the image's home registry. The registry is
        /// determined based on the image's name, defaulting to "docker.io".
        /// </remarks>
        /// <exception cref="ArgumentException"><paramref name="image"/> is null.</exception>
        /// <exception cref="MalformedReferenceException">The input could not be parsed as an image name.</exception>
        /// <exception cref="InvalidOperationException">
        /// One Task removed the auth object while another was getting it.
        /// </exception>
        /// <exception cref="RegistryAuthException">
        /// The registry requires credentials that the client hasn't been given.
        /// </exception>
        /// <exception cref="System.Net.Http.HttpRequestException">
        /// The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate
        /// validation, or timeout.
        /// </exception>
        public async Task<Image> PullAsync(ImageReference image, CancellationToken ct = default)
        {
            // Get the authentication information for the image's registry.
            var registryName = _docker.Registries.GetRegistryName(image);
            var auth = await _docker.Registries.GetAuthObjectAsync(registryName, ct).ConfigureAwait(false);

            // Perform the pull request.
            var request = new CoreModels.ImagesCreateParameters
            {
                FromImage = image,
            };
            try
            {
                await _docker.Core.Images.CreateImageAsync(request, auth, new NoOpProgress(), ct).ConfigureAwait(false);
            }
            catch (Core.DockerApiException ex)
            {
                if (ImageNotFoundException.TryWrap(ex, image, out var nex))
                    throw nex;
                if (RegistryAuthException.TryWrap(ex, registryName, out nex))
                    throw nex;
                throw DockerException.Wrap(ex);
            }

            // Return an object representing the new image.
            return await GetAsync(image, ct).ConfigureAwait(false);
        }

        private static IDictionary<string, IDictionary<string, bool>> MakeListFilters(ListImagesOptions options)
        {
            var output = new Dictionary<string, IDictionary<string, bool>>();

            if (options.DanglingImagesFilter == true)
                output.Add("dangling", new Dictionary<string, bool> { ["true"] = true });
            else if (options.DanglingImagesFilter == false)
                output.Add("dangling", new Dictionary<string, bool> { ["false"] = true });

            var labelsDictionary = new Dictionary<string, bool>();
            foreach (var label in options.LabelExistsFilters)
                labelsDictionary.Add(label, true);
            foreach (var (label, value) in options.LabelValueFilters)
                labelsDictionary.Add($"{label}={value}", true);
            if (labelsDictionary.Any())
                output.Add("label", labelsDictionary);

            var globsDictionary = new Dictionary<string, bool>();
            foreach (var glob in options.ReferencePatternFilters)
                globsDictionary.Add(glob, true);
            if (globsDictionary.Any())
                output.Add("reference", globsDictionary);

            return output;
        }

        private class NoOpProgress : IProgress<CoreModels.JSONMessage>
        {
            public void Report(CoreModels.JSONMessage value)
            {
            }
        }

        // TODO: PushAsync

        // TODO: RemoveAsync

        // TODO: RemoveUnusedAsync (docker image prune)

        // TODO: TagAsync (docker image tag)
    }
}
