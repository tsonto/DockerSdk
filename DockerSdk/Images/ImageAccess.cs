using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using DockerSdk.Builders;
using DockerSdk.Images.Events;
using DockerSdk.Registries;
using DockerSdk.Core;
using CoreModels = DockerSdk.Core.Models;

namespace DockerSdk.Images
{
    /// <summary>
    /// Provides methods for interacting with Docker images.
    /// </summary>
    public class ImageAccess : IObservable<ImageEvent>
    {
        internal ImageAccess(DockerClient docker)
        {
            _docker = docker;
            Builder = new Builder(docker);
        }

        private readonly DockerClient _docker;

        /// <summary>
        /// Gets a service for building Docker images.
        /// </summary>
        public Builder Builder { get; }


        // TODO: ExportAsync (docker image save)

        /// <summary>
        /// Loads an object that can be used to interact with the indicated image.
        /// </summary>
        /// <param name="image">An image name or ID.</param>
        /// <param name="ct">A token used to cancel the operation.</param>
        /// <returns>A <see cref="Task{TResult}"/> that resolves to the image object.</returns>
        /// <exception cref="ImageNotFoundLocallyException">No such image exists.</exception>
        /// <exception cref="System.Net.Http.HttpRequestException">
        /// The request failed due to an underlying issue such as loss of network connectivity.
        /// </exception>
        /// <exception cref="MalformedReferenceException">The image reference is improperly formatted.</exception>
        public Task<IImage> GetAsync(string image, CancellationToken ct = default)
            => GetAsync(ImageReference.Parse(image), ct);

        /// <summary>
        /// Loads an object that can be used to interact with the indicated image.
        /// </summary>
        /// <param name="image">An image name or ID.</param>
        /// <param name="ct">A token used to cancel the operation.</param>
        /// <returns>A <see cref="Task{TResult}"/> that resolves to the image object.</returns>
        /// <exception cref="ImageNotFoundLocallyException">No such image exists.</exception>
        /// <exception cref="System.Net.Http.HttpRequestException">
        /// The request failed due to an underlying issue such as loss of network connectivity.
        /// </exception>
        public Task<IImage> GetAsync(ImageReference image, CancellationToken ct = default)
        {
            if (image is null)
                throw new ArgumentNullException(nameof(image));

            return ImageFactory.LoadAsync(_docker, image, ct);
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
        public Task<IImageInfo> GetDetailsAsync(string image, CancellationToken ct = default)
            => GetDetailsAsync(ImageReference.Parse(image), ct);

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
        public Task<IImageInfo> GetDetailsAsync(ImageReference image, CancellationToken ct = default)
            => ImageFactory.LoadInfoAsync(_docker, image, ct);

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
        public Task<IReadOnlyList<IImage>> ListAsync(CancellationToken ct = default)
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
        public async Task<IReadOnlyList<IImage>> ListAsync(ListImagesOptions options, CancellationToken ct = default)
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
                response = await _docker.Comm.Images.ListImagesAsync(request, ct).ConfigureAwait(false);
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
        /// <exception cref="ImageNotFoundRemotelyException">
        /// The image does not exist at the Registry indicated by its name.
        /// </exception>
        public Task<IImage> PullAsync(string image, CancellationToken ct = default)
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
        /// <exception cref="ImageNotFoundRemotelyException">
        /// The image does not exist at the Registry indicated by its name.
        /// </exception>
        public async Task<IImage> PullAsync(ImageReference image, CancellationToken ct = default)
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
                await _docker.Comm.Images.CreateImageAsync(request, auth, new NoOpProgress(), ct).ConfigureAwait(false);
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

        private static IDictionary<string, IDictionary<string, bool>>? MakeListFilters(ListImagesOptions options)
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

            if (!output.Any())
                return null;
            return output;
        }

        /// <summary>
        /// Subscribes to events about images.
        /// </summary>
        /// <param name="observer">An object to observe the events.</param>
        /// <returns>
        /// An <see cref="IDisposable"/> representing the subscription. Disposing this unsubscribes and releases
        /// resources.
        /// </returns>
        public IDisposable Subscribe(IObserver<ImageEvent> observer)
            => _docker.OfType<ImageEvent>().Subscribe(observer);

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
