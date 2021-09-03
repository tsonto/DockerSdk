using System;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using DockerSdk.Containers;
using DockerSdk.Images.Events;

namespace DockerSdk.Images
{
    /// <summary>
    /// Represents a Docker image, which is a read-only template for creating containers.
    /// </summary>
    /// <seealso cref="ImageInfo"/>
    internal class Image : IImage
    {
        /// <summary>
        /// Creates an instance of the <see cref="Image"/> class.
        /// </summary>
        /// <param name="client">The <see cref="DockerClient"/> instance to use.</param>
        /// <param name="id">The image's full ID. If the hash function is not specified, SHA-256 is assumed.S</param>
        internal Image(DockerClient client, ImageFullId id)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            Id = id ?? throw new ArgumentNullException(nameof(id));
        }

        /// <inheritdoc/>
        public ImageFullId Id { get; }

        private readonly DockerClient _client;

        /// <inheritdoc/>
        public Task<IImageInfo> GetDetailsAsync(CancellationToken ct = default)
            => ImageFactory.LoadInfoAsync(_client, Id, ct);

        /// <inheritdoc/>
        public Task<IContainer> RunAsync(CancellationToken ct = default)
            => RunAsync(new CreateContainerOptions(), ct);

        /// <inheritdoc/>
        public Task<IContainer> RunAsync(CreateContainerOptions options, CancellationToken ct = default)
            => _client.Containers.CreateAndStartAsync(Id, options, ct);

        /// <summary>
        /// Subscribes to events from this image.
        /// </summary>
        /// <param name="observer">An object to observe the events.</param>
        /// <returns>
        /// An <see cref="IDisposable"/> representing the subscription. Disposing this unsubscribes and releases
        /// resources.
        /// </returns>
        public IDisposable Subscribe(IObserver<ImageEvent> observer)
            => _client.Images.Where((Func<ImageEvent, bool>)(ev => ev.ImageReference == Id)).Subscribe(observer);

        // TODO: PushAsync
        // TODO: RemoveAsync
        // TODO: ExportAsync (`docker image save` - to stream or file)
        // TODO: TagAsync
    }
}
