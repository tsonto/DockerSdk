using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using DockerSdk.Networks.Events;
using DockerSdk.Core;
using CoreModels = DockerSdk.Core.Models;
using System.Net.Http;
using DockerSdk.Core.Models;

namespace DockerSdk.Networks
{
    /// <summary>
    /// Provides methods for interacting with Docker networks.
    /// </summary>
    public class NetworkAccess : IObservable<NetworkEvent>
    {
        internal NetworkAccess(DockerClient dockerClient)
        {
            client = dockerClient;
        }

        private readonly DockerClient client;

        /// <summary>
        /// Subscribes to events about networks.
        /// </summary>
        /// <param name="observer">An object to observe the events.</param>
        /// <returns>
        /// An <see cref="IDisposable"/> representing the subscription. Disposing this unsubscribes and releases
        /// resources.
        /// </returns>
        public IDisposable Subscribe(IObserver<NetworkEvent> observer)
            => client.OfType<NetworkEvent>().Subscribe(observer);

        /// <summary>
        /// Loads an object that can be used to interact with the indicated network.
        /// </summary>
        /// <param name="network">A network name or ID.</param>
        /// <param name="ct">A token used to cancel the operation.</param>
        /// <returns>A <see cref="Task{TResult}"/> that resolves to the network object.</returns>
        /// <exception cref="NetworkNotFoundException">No such network exists.</exception>
        /// <exception cref="System.Net.Http.HttpRequestException">
        /// The request failed due to an underlying issue such as loss of network connectivity.
        /// </exception>
        /// <exception cref="MalformedReferenceException">The network reference is improperly formatted.</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="network"/> input is null.</exception>
        public Task<INetwork> GetAsync(string network, CancellationToken ct = default)
            => GetAsync(NetworkReference.Parse(network), ct);

        /// <summary>
        /// Loads an object that can be used to interact with the indicated network.
        /// </summary>
        /// <param name="network">A network name or ID.</param>
        /// <param name="ct">A token used to cancel the operation.</param>
        /// <returns>A <see cref="Task{TResult}"/> that resolves to the network object.</returns>
        /// <exception cref="NetworkNotFoundException">No such network exists.</exception>
        /// <exception cref="System.Net.Http.HttpRequestException">
        /// The request failed due to an underlying issue such as loss of network connectivity.
        /// </exception>
        /// <exception cref="ArgumentNullException">The <paramref name="network"/> input is null.</exception>
        public async Task<INetwork> GetAsync(NetworkReference network, CancellationToken ct = default)
        {
            if (network is null)
                throw new ArgumentNullException(nameof(network));

            return await NetworkFactory.LoadAsync(client, network, ct).ConfigureAwait(false);
        }

        internal async Task<NetworkFullId> ToFullIdAsync(NetworkReference input, CancellationToken ct)
        {
            if (input is NetworkFullId nfid)
                return nfid;

            var network = await GetAsync(input, ct).ConfigureAwait(false);
            return network.Id;
        }

        /// <summary>
        /// Loads detailed information about a Docker network.
        /// </summary>
        /// <param name="network">A network name or ID.</param>
        /// <param name="ct">A token used to cancel the operation.</param>
        /// <returns>A <see cref="Task{TResult}"/> that resolves to the details.</returns>
        /// <exception cref="NetworkNotFoundException">No such network exists.</exception>
        /// <exception cref="System.Net.Http.HttpRequestException">
        /// The request failed due to an underlying issue such as network connectivity.
        /// </exception>
        /// <exception cref="MalformedReferenceException">The network reference is improperly formatted.</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="network"/> input is null.</exception>
        public Task<INetworkInfo> GetInfoAsync(string network, CancellationToken ct = default)
            => GetInfoAsync(NetworkReference.Parse(network), ct);

        /// <summary>
        /// Loads detailed information about a Docker network.
        /// </summary>
        /// <param name="network">A network name or ID.</param>
        /// <param name="ct">A token used to cancel the operation.</param>
        /// <returns>A <see cref="Task{TResult}"/> that resolves to the details.</returns>
        /// <exception cref="NetworkNotFoundException">No such network exists.</exception>
        /// <exception cref="System.Net.Http.HttpRequestException">
        /// The request failed due to an underlying issue such as network connectivity.
        /// </exception>
        /// <exception cref="ArgumentNullException">An input is null.</exception>
        public async Task<INetworkInfo> GetInfoAsync(NetworkReference network, CancellationToken ct = default)
        {
            if (network is null)
                throw new ArgumentNullException(nameof(network));

            return await NetworkFactory.LoadInfoAsync(client, network, ct).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets a list of Docker networks known to the daemon.
        /// </summary>
        /// <param name="ct">A token used to cancel the operation.</param>
        /// <returns>A <see cref="Task{TResult}"/> that resolves to the list of images.</returns>
        /// <remarks>The sequence of the results is undefined.</remarks>
        /// <exception cref="System.Net.Http.HttpRequestException">
        /// The request failed due to an underlying issue such as network connectivity.
        /// </exception>
        /// <exception cref="ArgumentNullException">An input is null.</exception>
        public Task<IReadOnlyList<INetwork>> ListAsync(CancellationToken ct = default)
            => ListAsync(new(), ct);

        /// <summary>
        /// Gets a list of Docker networks known to the daemon.
        /// </summary>
        /// <param name="options">Filters for the search.</param>
        /// <param name="ct">A token used to cancel the operation.</param>
        /// <returns>A <see cref="Task{TResult}"/> that resolves to the list of images.</returns>
        /// <remarks>The sequence of the results is undefined.</remarks>
        /// <exception cref="System.Net.Http.HttpRequestException">
        /// The request failed due to an underlying issue such as network connectivity.
        /// </exception>
        /// <exception cref="ArgumentNullException">An input is null.</exception>
        public async Task<IReadOnlyList<INetwork>> ListAsync(ListNetworksOptions options, CancellationToken ct = default)
        {
            // Validate the input.
            if (options is null)
                throw new ArgumentNullException(nameof(options));

            // Send the API request.
            var response = await client.BuildRequest(HttpMethod.Get, "networks")
                .WithParameters(options.ToQueryString())
                .SendAsync<NetworkResponse[]>(ct)
                .ConfigureAwait(false);

            // Convert the response into network objects.
            return response
                .Select(net => new Network(client, new NetworkFullId(net.Id)))
                .ToArray();
        }
    }
}
