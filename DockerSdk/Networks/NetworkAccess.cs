using System;
using System.Collections.Generic;
using System.Linq;
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

            // Construct the API request.
            var request = new CoreModels.NetworksListParameters
            {
                Filters = MakeListFilters(options, out bool none),
            };
            if (none)
                return Array.Empty<INetwork>();

            // Send the API request.
            IList<CoreModels.NetworkResponse> response;
            try
            {
                response = await client.Core.Networks.ListNetworksAsync(request, ct).ConfigureAwait(false);
            }
            catch (Core.DockerApiException ex)
            {
                throw DockerException.Wrap(ex);
            }

            // Convert the response into network objects.
            return response
                .Select(net => new Network(client, new NetworkFullId(net.ID)))
                .ToArray();
        }

        private static IDictionary<string, IDictionary<string, bool>>? MakeListFilters(ListNetworksOptions options, out bool none)
        {
            var output = new Dictionary<string, IDictionary<string, bool>>();
            none = false;

            if (options.DanglingNetworksFilter == true)
                output.Add("dangling", new Dictionary<string, bool> { ["true"] = true });
            else if (options.DanglingNetworksFilter == false)
                output.Add("dangling", new Dictionary<string, bool> { ["false"] = true });

            if (options.BuiltInNetworksFilter == true)
                output.Add("type", new Dictionary<string, bool> { ["builtin"] = true });
            else if (options.BuiltInNetworksFilter == false)
                output.Add("type", new Dictionary<string, bool> { ["custom"] = true });

            if (options.ScopeFilter is not null)
            {
                if (options.ScopeFilter == NetworkScope.Local)
                    output.Add("scope", new Dictionary<string, bool> { ["local"] = true });
                else if (options.ScopeFilter == NetworkScope.Swarm)
                    output.Add("scope", new Dictionary<string, bool> { ["swarm"] = true });
                else
                    throw new NotImplementedException($"Scope type {options.ScopeFilter} is not supported for filtering networks.");
            }

            var labelsDictionary = new Dictionary<string, bool>();
            foreach (var label in options.LabelExistsFilters)
                labelsDictionary.Add(label, true);
            foreach (var (label, value) in options.LabelValueFilters)
                labelsDictionary.Add($"{label}={value}", true);
            if (labelsDictionary.Any())
                output.Add("label", labelsDictionary);

            if (options.DriverFilter is not null)
            {
                var driversDict = new Dictionary<string, bool>();
                foreach (var driverName in options.DriverFilter)
                    driversDict.Add(driverName, true);
                if (driversDict.Any())
                    output.Add("driver", driversDict);
                else
                    none = true;
            }

            if (options.IdFilter is not null)
            {
                var idsDict = new Dictionary<string, bool>();
                foreach (var id in options.IdFilter)
                    idsDict.Add(id, true);
                if (idsDict.Any())
                    output.Add("id", idsDict);
                else
                    none = true;
            }

            if (options.NameFilter is not null)
            {
                var idsDict = new Dictionary<string, bool>();
                foreach (var name in options.NameFilter)
                    idsDict.Add(name, true);
                if (idsDict.Any())
                    output.Add("name", idsDict);
                else
                    none = true;
            }

            if (!output.Any())
                return null;
            return output;
        }
    }
}
