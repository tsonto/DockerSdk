using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DockerSdk.Images;
using Core = Docker.DotNet;
using CoreModels = Docker.DotNet.Models;

namespace DockerSdk.Containers
{
    /// <summary>
    /// Provides access to functionality involving Docker containers.
    /// </summary>
    /// <remarks>
    /// Not all container functionality is available directly on this class. See <see cref="Container"/> for more
    /// functionality.
    /// </remarks>
    public class ContainerAccess
    {
        internal ContainerAccess(DockerClient client)
        {
            _docker = client;
        }

        private readonly DockerClient _docker;

        /// <summary>
        /// Loads an object that can be used to interact with the indicated container.
        /// </summary>
        /// <param name="container">A container name or ID.</param>
        /// <param name="ct">A token used to cancel the operation.</param>
        /// <returns>A <see cref="Task{TResult}"/> that resolves to the container object.</returns>
        /// <exception cref="ContainerNotFoundException">No such container exists.</exception>
        /// <exception cref="System.Net.Http.HttpRequestException">
        /// The request failed due to an underlying issue such as loss of network connectivity.
        /// </exception>
        /// <exception cref="MalformedReferenceException">The container reference is improperly formatted.</exception>
        /// <remarks>
        /// Caution: Container names and short IDs are not guaranteed to be unique. If there is more than one match for
        /// the reference, the result is undefined.
        /// </remarks>
        public Task<Container> GetAsync(string container, CancellationToken ct = default)
            => GetAsync(ContainerReference.Parse(container), ct);

        /// <summary>
        /// Loads an object that can be used to interact with the indicated container.
        /// </summary>
        /// <param name="container">A container name or ID.</param>
        /// <param name="ct">A token used to cancel the operation.</param>
        /// <returns>A <see cref="Task{TResult}"/> that resolves to the container object.</returns>
        /// <exception cref="ContainerNotFoundException">No such container exists.</exception>
        /// <exception cref="System.Net.Http.HttpRequestException">
        /// The request failed due to an underlying issue such as loss of network connectivity.
        /// </exception>
        /// <remarks>
        /// Caution: Container names and short IDs are not guaranteed to be unique. If there is more than one match for
        /// the reference, the result is undefined.
        /// </remarks>
        public async Task<Container> GetAsync(ContainerReference container, CancellationToken ct = default)
        {
            if (container is null)
                throw new ArgumentNullException(nameof(container));

            CoreModels.ContainerInspectResponse response;
            try
            {
                response = await _docker.Core.Containers.InspectContainerAsync(container, ct).ConfigureAwait(false);
            }
            catch (Core.DockerContainerNotFoundException ex)
            {
                throw new ContainerNotFoundException($"No such container \"{container}\".", ex);
            }
            catch (Core.DockerApiException ex)
            {
                throw DockerException.Wrap(ex);
            }

            return new Container(_docker, new ContainerFullId(response.ID));
        }

        public Task<ContainerDetails> GetDetailsAsync(string container, CancellationToken ct = default)
                    => GetDetailsAsync(ContainerReference.Parse(container), ct);

        public async Task<ContainerDetails> GetDetailsAsync(ContainerReference container, CancellationToken ct = default)
        {
            if (container is null)
                throw new ArgumentNullException(nameof(container));

            CoreModels.ContainerInspectResponse response;
            try
            {
                response = await _docker.Core.Containers.InspectContainerAsync(container, ct).ConfigureAwait(false);
            }
            catch (Core.DockerContainerNotFoundException ex)
            {
                throw new ContainerNotFoundException($"No such container \"{container}\".", ex);
            }
            catch (Core.DockerApiException ex)
            {
                throw DockerException.Wrap(ex);
            }

            return new ContainerDetails(response);
        }

        /// <summary>
        /// Gets a list of Docker containers known to the daemon.
        /// </summary>
        /// <param name="ct">A token used to cancel the operation.</param>
        /// <returns>A <see cref="Task{TResult}"/> that resolves to the list of containers.</returns>
        /// <remarks>
        /// <para>
        /// This method does not necessarily return the same results as the `docker image ls` command. To get the same
        /// results, use the <see cref="ListAsync(ListContainersOptions, CancellationToken)"/> overload and give it the
        /// <see cref="ListContainersOptions.CommandLineDefaults"/> options.
        /// </para>
        /// <para>The sequence of the results is undefined.</para>
        /// </remarks>
        public Task<IReadOnlyList<Container>> ListAsync(CancellationToken ct = default)
            => ListAsync(new ListContainersOptions(), ct);

        /// <summary>
        /// Gets a list of Docker containers known to the daemon.
        /// </summary>
        /// <param name="options">Filters for the search.</param>
        /// <param name="ct">A token used to cancel the operation.</param>
        /// <returns>A <see cref="Task{TResult}"/> that resolves to the list of containers.</returns>
        /// <remarks>
        /// <para>
        /// This method does not necessarily return the same results as the `docker image ls` command. To get the same
        /// results, pass in <see cref="ListContainersOptions.CommandLineDefaults"/>.
        /// </para>
        /// <para>The sequence of the results is undefined.</para>
        /// </remarks>
        public async Task<IReadOnlyList<Container>> ListAsync(ListContainersOptions options, CancellationToken ct = default)
        {
            if (options is null)
                throw new ArgumentNullException(nameof(options));

            var request = new CoreModels.ContainersListParameters
            {
                All = !options.OnlyRunningContainers,
                Filters = MakeFilters(options),
                Limit = options.MaxResults,
                Size = false,
            };

            IList<CoreModels.ContainerListResponse> response;
            try
            {
                response = await _docker.Core.Containers.ListContainersAsync(request, ct).ConfigureAwait(false);
            }
            catch (Core.DockerApiException ex)
            {
                throw DockerException.Wrap(ex);
            }

            return response
                .Select(r => new Container(_docker, new(r.ID)))
                .ToImmutableArray();
        }

        private IDictionary<string, IDictionary<string, bool>>? MakeFilters(ListContainersOptions options)
        {
            var output = new Dictionary<string, IDictionary<string, bool>>();

            // Build the "ancestor" filter.
            if (!string.IsNullOrEmpty(options.AncestorFilter))
            {
                _ = ImageReference.Parse(options.AncestorFilter);
                var subdict = new Dictionary<string, bool>() { [options.AncestorFilter] = true };
                output.Add("ancestor", subdict);
            }

            // Build the "exited" filter.
            if (options.ExitCodeFilter.HasValue)
            {
                var exitCodeString = options.ExitCodeFilter.Value.ToString(CultureInfo.InvariantCulture);
                var subdict = new Dictionary<string, bool>() { [exitCodeString] = true };
                output.Add("exited", subdict);
            }

            // Build the "label" filter. This comes from two settings, LabelExistsFilters and LabelValueFilters.
            var labelsDictionary = new Dictionary<string, bool>();
            foreach (var label in options.LabelExistsFilters)
                labelsDictionary.Add(label, true);
            foreach (var (label, value) in options.LabelValueFilters)
                labelsDictionary.Add($"{label}={value}", true);
            if (labelsDictionary.Any())
                output.Add("label", labelsDictionary);

            // Build the "name" filter. This is a glob pattern.
            if (!string.IsNullOrEmpty(options.NameFilter))
            {
                var subdict = new Dictionary<string, bool>() { [options.NameFilter] = true };
                output.Add("name", subdict);
            }

            // Build the "status" filter.
            if (options.StatusFilter.HasValue)
            {
                var statusString = options.StatusFilter.Value.ToString().ToLowerInvariant();
                var subdict = new Dictionary<string, bool>() { [statusString] = true };
                output.Add("status", subdict);
            }

            // Note: This is not all available filters. As of 4/2021, these other filters exist but are not implemented
            // here: before, expose, health, id, isolation, is-task, network, publish, since, volume.

            if (!output.Any())
                return null;
            return output;
        }

        // TODO: CreateAsync
        // TODO: StartAsync
        // TODO: StopAsync, optionally combine with wait
        // TODO: RestartAsync
        // TODO: KillAsync
        // TODO: RenameAsync
        // TODO: PauseAsync
        // TODO: UnpauseAsync
        // TODO: RemoveAsync
        // TODO: PruneAsync
    }
}
