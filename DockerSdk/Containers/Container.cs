using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using DockerSdk.Containers.Events;

namespace DockerSdk.Containers
{
    /// <summary>
    /// Represents a Docker container.
    /// </summary>
    internal class Container : IObservable<ContainerEvent>, IContainer
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
        public Task<IContainerInfo> GetDetailsAsync(CancellationToken ct)
            => GetDetailsAsync(ContainerLoadOptions.Shallow, ct);

        /// <inheritdoc/>
        public Task<IContainerInfo> GetDetailsAsync(ContainerLoadOptions options, CancellationToken ct)
            => this is IContainerInfo info
            ? Task.FromResult(info)
            : _client.Containers.GetDetailsAsync(Id, options, ct);

        /// <inheritdoc/>
        public Task StartAsync(CancellationToken ct = default)
            => _client.Containers.StartAsync(Id, ct);

        /// <inheritdoc/>
        public IDisposable Subscribe(IObserver<ContainerEvent> observer)
            => _client.Containers.Where(ev => ev.ContainerId == Id).Subscribe(observer);
    }
}
