using System;
using System.Threading;
using System.Threading.Tasks;
using DockerSdk.Containers;

namespace DockerSdk.Images
{
    /// <summary>
    /// Represents a Docker image, which is a read-only template for creating containers.
    /// </summary>
    /// <seealso cref="ImageDetails"/>
    public class Image
    {
        /// <summary>
        /// Creates an instance of the <see cref="Image"/> class.
        /// </summary>
        /// <param name="client">The <see cref="Docker"/> instance to use.</param>
        /// <param name="id">The image's full ID. If the hash function is not specified, SHA-256 is assumed.S</param>
        internal Image(DockerClient client, ImageFullId id)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            Id = id ?? throw new ArgumentNullException(nameof(id));
        }

        /// <summary>
        /// Gets the image's full ID.
        /// </summary>
        public ImageFullId Id { get; }

        private readonly DockerClient _client;

        /// <summary>
        /// Gets detailed information about the image.
        /// </summary>
        /// <param name="ct">A <see cref="CancellationToken"/> used to cancel the operation.</param>
        /// <returns>A <see cref="Task"/> that completes when the result is available.</returns>
        /// <exception cref="InvalidOperationException">The image no longer exists.</exception>
        /// <exception cref="System.Net.Http.HttpRequestException">
        /// The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate
        /// validation, or timeout.
        /// </exception>
        public Task<ImageDetails> GetDetailsAsync(CancellationToken ct = default)
            => ImageDetails.LoadAsync(_client, Id, ct);

        /// <summary>
        /// Creates a new container from this image and starts it.
        /// </summary>
        /// <param name="ct"></param>
        /// <returns>A <see cref="Task{TResult}"/> that resolves when the container has started. It resolves to the new container.</returns>
        /// <remarks>This method does <em>not</em> wait for the container to finish.</remarks>
        public Task<Container> RunAsync(CancellationToken ct = default)
            => RunAsync(new CreateContainerOptions(), ct);

        /// <summary>
        /// Creates a new container from this image and starts it.
        /// </summary>
        /// <param name="options">Settings for the new container.</param>
        /// <param name="ct"></param>
        /// <returns>A <see cref="Task{TResult}"/> that resolves when the container has started. It resolves to the new container.</returns>
        /// <remarks>This method does <em>not</em> wait for the container to finish.</remarks>
        public Task<Container> RunAsync(CreateContainerOptions options, CancellationToken ct = default)
            => _client.Containers.CreateAndStartAsync(Id, options, ct);

        // TODO: PushAsync
        // TODO: RemoveAsync
        // TODO: ExportAsync (`docker image save` - to stream or file)
        // TODO: TagAsync
    }
}
