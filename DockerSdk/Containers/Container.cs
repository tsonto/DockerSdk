using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using DockerSdk.Containers.Events;
using DockerSdk.Networks;

namespace DockerSdk.Containers
{
    /// <summary>
    /// Represents a Docker container.
    /// </summary>
    internal class Container : IContainer
    {
        internal Container(DockerClient client, ContainerFullId id)
        {
            _client = client;
            Id = id;
        }

        /// <inheritdoc/>
        public ContainerFullId Id { get; }

        private readonly DockerClient _client;

        /// <inheritdoc/>
        public Task AttachNetwork(string network, CancellationToken ct = default)
            => AttachNetwork(NetworkReference.Parse(network), new(), ct);

        /// <inheritdoc/>
        public Task AttachNetwork(NetworkReference network, CancellationToken ct = default)
            => AttachNetwork(network, new(), ct);

        /// <inheritdoc/>
        public Task AttachNetwork(string network, AttachNetworkOptions options, CancellationToken ct = default)
            => AttachNetwork(NetworkReference.Parse(network), options, ct);

        /// <inheritdoc/>
        public async Task AttachNetwork(NetworkReference network, AttachNetworkOptions options, CancellationToken ct = default)
        {
            if (network is null)
                throw new ArgumentNullException(nameof(network));
            if (options is null)
                throw new ArgumentNullException(nameof(options));

            await Network.AttachInnerAsync(_client, Id, network, options, ct).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public Task<IContainerInfo> GetDetailsAsync(CancellationToken ct)
            => GetDetailsAsync(ct);

        /// <inheritdoc/>
        public Task StartAsync(CancellationToken ct = default)
            => _client.Containers.StartAsync(Id, ct);

        /// <summary>
        /// Subscribes to events from this container.
        /// </summary>
        /// <param name="observer">An object to observe the events.</param>
        /// <returns>
        /// An <see cref="IDisposable"/> representing the subscription. Disposing this unsubscribes and releases
        /// resources.
        /// </returns>
        public IDisposable Subscribe(IObserver<ContainerEvent> observer)
            => _client.Containers.Where(ev => ev.ContainerId == Id).Subscribe(observer);
    }
}
