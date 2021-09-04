using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using DockerSdk.Containers;
using DockerSdk.Networks.Events;
using System.Net.Http;
using System.Net;

namespace DockerSdk.Networks
{
    /// <summary>
    /// Represents a Docker network.
    /// </summary>
    /// <seealso cref="NetworkInfo"/>
    internal class Network : INetwork
    {
        internal Network(DockerClient client, NetworkFullId id)
        {
            this.client = client;
            Id = id;
        }

        /// <inheritdoc/>
        public NetworkFullId Id { get; }

        private readonly DockerClient client;

        public Task AttachAsync(string container, CancellationToken ct = default)
            => AttachAsync(ContainerReference.Parse(container), new(), ct);

        public Task AttachAsync(ContainerReference container, CancellationToken ct = default)
            => AttachAsync(container, new(), ct);

        public Task AttachAsync(string container, AttachNetworkOptions options, CancellationToken ct = default)
            => AttachAsync(ContainerReference.Parse(container), options, ct);

        public async Task AttachAsync(ContainerReference container, AttachNetworkOptions options, CancellationToken ct = default)
        {
            if (container is null)
                throw new ArgumentNullException(nameof(container));
            if (options is null)
                throw new ArgumentNullException(nameof(options));

            await AttachInnerAsync(client, container, Id, options, ct).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public Task<INetworkInfo> GetDetailsAsync(CancellationToken ct = default)
            => this is INetworkInfo info
            ? Task.FromResult(info)
            : client.Networks.GetInfoAsync(Id, ct);

        /// <summary>
        /// Attaches a Docker network to a Docker container.
        /// </summary>
        /// <param name="client">The Docker client to use.</param>
        /// <param name="container">A reference to the container.</param>
        /// <param name="network">A reference to the network.</param>
        /// <param name="options">Options for how to perform the operation.</param>
        /// <param name="ct">A token used to cancel the operation.</param>
        /// <returns>A <see cref="Task"/> that resolves when the network has been attached.</returns>
        /// <exception cref="ArgumentException">The <paramref name="options"/> input has invalid values, such as an IPv6 address in the <see cref="AttachNetworkOptions.IPv6Address"/> property.</exception>
        /// <exception cref="NetworkNotFoundException">The indicated network does not exist.</exception>
        /// <exception cref="ContainerNotFoundException">The indicated container does not exist.</exception>
        /// <exception cref="System.Net.Http.HttpRequestException">
        /// The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate
        /// validation, or timeout.
        /// </exception>
        internal static async Task AttachInnerAsync(DockerClient client, ContainerReference container, NetworkReference network, AttachNetworkOptions options, CancellationToken ct)
        {
            if (options.IPv4Address is not null && options.IPv4Address?.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork)
                throw new ArgumentException($"{nameof(options.IPv4Address)} has a non-IPv4 address.");
            if (options.IPv6Address is not null && options.IPv6Address?.AddressFamily != System.Net.Sockets.AddressFamily.InterNetworkV6)
                throw new ArgumentException($"{nameof(options.IPv6Address)} has a non-IPv6 address.");

            // The event match condition needs the full IDs, so fetch them if we don't have them already.
            var cfid = await client.Containers.ToFullIdAsync(container, ct).ConfigureAwait(false);
            var nfid = await client.Networks.ToFullIdAsync(network, ct).ConfigureAwait(false);

            // Start watching for the event that means the operation is done.
            var found = new TaskCompletionSource<NetworkEvent>();
            using var subscription = client.Networks
                .OfType<NetworkAttachedEvent>()
                .Where(ev => ev.NetworkId == nfid && ev.ContainerId == cfid)
                .Subscribe(ev => found.SetResult(ev));

            await client.BuildRequest(HttpMethod.Post, $"networks/{nfid}/connect")
                .WithJsonBody(options.ToBodyObject(container))
                .AcceptStatus(HttpStatusCode.OK)
                .RejectStatus(HttpStatusCode.NotFound, $"No such container: {container}", _ => new ContainerNotFoundException($"Container \"{container}\" does not exist."))
                // TODO: NetworkNotFoundException
                .SendAsync(ct)
                .ConfigureAwait(false);

            // Wait until we receive the confirmation message.
            await found.Task.ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public IDisposable Subscribe(IObserver<NetworkEvent> observer)
            => client.Networks.Where(ev => ev.NetworkId == Id).Subscribe(observer);
    }
}
