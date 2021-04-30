using System;
using System.Threading;
using System.Threading.Tasks;
using Core = Docker.DotNet;
using CoreModels = Docker.DotNet.Models;

namespace DockerSdk.Networks
{
    /// <summary>
    /// Provides methods for interacting with Docker networks.
    /// </summary>
    public class NetworkAccess
    {
        internal NetworkAccess(DockerClient dockerClient)
        {
            client = dockerClient;
        }

        private readonly DockerClient client;

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
        public Task<Network> GetAsync(string network, CancellationToken ct = default)
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
        public async Task<Network> GetAsync(NetworkReference network, CancellationToken ct = default)
        {
            CoreModels.NetworkResponse response;
            try
            {
                response = await client.Core.Networks.InspectNetworkAsync(network, ct).ConfigureAwait(false);
            }
            catch (Core.DockerApiException ex)
            {
                if (NetworkNotFoundException.TryWrap(ex, network, out var nex))
                    throw nex;
                throw DockerException.Wrap(ex);
            }

            return new Network(client, new NetworkFullId(response.ID));
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
        public Task<NetworkDetails> GetDetailsAsync(string network, CancellationToken ct = default)
            => GetDetailsAsync(NetworkReference.Parse(network), ct);

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
        /// <exception cref="ArgumentNullException">The <paramref name="network"/> input is null.</exception>
        public async Task<NetworkDetails> GetDetailsAsync(NetworkReference network, CancellationToken ct = default)
        {
            CoreModels.NetworkResponse response;
            try
            {
                response = await client.Core.Networks.InspectNetworkAsync(network, ct).ConfigureAwait(false);
            }
            catch (Core.DockerApiException ex)
            {
                if (NetworkNotFoundException.TryWrap(ex, network, out var nex))
                    throw nex;
                throw DockerException.Wrap(ex);
            }

            return new NetworkDetails(client, response);
        }
    }
}
