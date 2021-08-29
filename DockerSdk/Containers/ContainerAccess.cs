using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using DockerSdk.Containers.Events;
using DockerSdk.Images;
using DockerSdk.Networks;
using DockerSdk.Registries;
using DockerSdk.Core;
using CoreModels = DockerSdk.Core.Models;

namespace DockerSdk.Containers
{
    /// <summary>
    /// Provides access to functionality involving Docker containers.
    /// </summary>
    /// <remarks>
    /// Not all container functionality is available directly on this class. See <see cref="Container"/> for more
    /// functionality.
    /// </remarks>
    public class ContainerAccess : IObservable<ContainerEvent>
    {
        internal ContainerAccess(DockerClient client)
        {
            _docker = client;
        }

        private readonly DockerClient _docker;

        /// <summary>
        /// Creates a Docker container and starts it.
        /// </summary>
        /// <param name="image">The image to create the container from.</param>
        /// <param name="ct">A token used to cancel the operation.</param>
        /// <returns>A <see cref="Task{TResult}"/> that resolves when the container's main process has started.</returns>
        /// <remarks>
        /// <para>
        /// From the perspective of Docker, there's no concept of whether the container's main process has "finished
        /// starting"--just that the process has been started at all. Thus, for example, if the process is a web server,
        /// this method's <c>Task</c> may resolve before the web server is ready for connections. If the application
        /// using this library needs to synchronize with events happening inside the container, it should monitor the
        /// container's logs or use other real-time mechanisms to do so.
        /// </para>
        /// <para>
        /// It's also possible that a short-lived process might exit before the method's <c>Task</c> resolves.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentException"><paramref name="image"/> is null or empty.</exception>
        /// <exception cref="ImageNotFoundLocallyException">
        /// The Docker daemon cannot find the image locally, and the <see cref="CreateContainerOptions.PullCondition"/>
        /// option is not set to pull it automatically.
        /// </exception>
        /// <exception cref="System.Net.Http.HttpRequestException">
        /// The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate
        /// validation, or timeout.
        /// </exception>
        /// <exception cref="MalformedReferenceException">
        /// The <paramref name="image"/> input is not well-formed.
        /// </exception>
        public Task<IContainer> CreateAndStartAsync(string image, CancellationToken ct = default)
            => CreateAndStartAsync(ImageReference.Parse(image), new CreateContainerOptions(), ct);

        /// <summary>
        /// Creates a Docker container and starts it.
        /// </summary>
        /// <param name="image">The image to create the container from.</param>
        /// <param name="ct">A token used to cancel the operation.</param>
        /// <returns>A <see cref="Task{TResult}"/> that resolves when the container's main process has started.</returns>
        /// <remarks>
        /// <para>
        /// From the perspective of Docker, there's no concept of whether the container's main process has "finished
        /// starting"--just that the process has been started at all. Thus, for example, if the process is a web server,
        /// this method's <c>Task</c> may resolve before the web server is ready for connections. If the application
        /// using this library needs to synchronize with events happening inside the container, it should monitor the
        /// container's logs or use other real-time mechanisms to do so.
        /// </para>
        /// <para>
        /// It's also possible that a short-lived process might exit before the method's <c>Task</c> resolves.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentException"><paramref name="image"/> is null.</exception>
        /// <exception cref="ImageNotFoundLocallyException">
        /// The Docker daemon cannot find the image locally, and the <see cref="CreateContainerOptions.PullCondition"/>
        /// option is not set to pull it automatically.
        /// </exception>
        /// <exception cref="System.Net.Http.HttpRequestException">
        /// The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate
        /// validation, or timeout.
        /// </exception>
        public Task<IContainer> CreateAndStartAsync(ImageReference image, CancellationToken ct = default)
            => CreateAndStartAsync(image, new CreateContainerOptions(), ct);

        /// <summary>
        /// Creates a Docker container and starts it.
        /// </summary>
        /// <param name="image">The image to create the container from.</param>
        /// <param name="options">Options for how to create the image and how it should behave.</param>
        /// <param name="ct">A token used to cancel the operation.</param>
        /// <returns>A <see cref="Task{TResult}"/> that resolves when the container's main process has started.</returns>
        /// <remarks>
        /// <para>
        /// From the perspective of Docker, there's no concept of whether the container's main process has "finished
        /// starting"--just that the process has been started at all. Thus, for example, if the process is a web server,
        /// this method's <c>Task</c> may resolve before the web server is ready for connections. If the application
        /// using this library needs to synchronize with events happening inside the container, it should monitor the
        /// container's logs or use other real-time mechanisms to do so.
        /// </para>
        /// <para>
        /// It's also possible that a short-lived process might exit before the method's <c>Task</c> resolves.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// <paramref name="image"/> and/or <paramref name="options"/> are null.
        /// </exception>
        /// <exception cref="ImageNotFoundLocallyException">
        /// The Docker daemon cannot find the image locally, and the <see cref="CreateContainerOptions.PullCondition"/>
        /// option is not set to pull it automatically.
        /// </exception>
        /// <exception cref="ImageNotFoundRemotelyException">
        /// The Docker image does not exist, even remotely. (Only applies when pulling an image, which is not enabled by
        /// default.)
        /// </exception>
        /// <exception cref="NetworkNotFoundException">One of the networks specified does not exist.</exception>
        /// <exception cref="MalformedReferenceException">
        /// The <paramref name="image"/> input is not well-formed. --or-- The options specified a name for the image,
        /// but the name does not meet the expectations of a well-formed container name.
        /// </exception>
        /// <exception cref="RegistryAuthException">
        /// The registry requires credentials that the client hasn't been given. (Only applies when pulling an image,
        /// which is not enabled by default.)
        /// </exception>
        /// <exception cref="System.Net.Http.HttpRequestException">
        /// The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate
        /// validation, or timeout.
        /// </exception>
        public Task<IContainer> CreateAndStartAsync(string image, CreateContainerOptions options, CancellationToken ct = default)
            => CreateAndStartAsync(ImageReference.Parse(image), options, ct);

        /// <summary>
        /// Creates a Docker container and starts it.
        /// </summary>
        /// <param name="image">The image to create the container from.</param>
        /// <param name="options">Options for how to create the image and how it should behave.</param>
        /// <param name="ct">A token used to cancel the operation.</param>
        /// <returns>A <see cref="Task{TResult}"/> that resolves when the container's main process has started.</returns>
        /// <remarks>
        /// <para>
        /// From the perspective of Docker, there's no concept of whether the container's main process has "finished
        /// starting"--just that the process has been started at all. Thus, for example, if the process is a web server,
        /// this method's <c>Task</c> may resolve before the web server is ready for connections. If the application
        /// using this library needs to synchronize with events happening inside the container, it should monitor the
        /// container's logs or use other real-time mechanisms to do so.
        /// </para>
        /// <para>
        /// It's also possible that a short-lived process might exit before the method's <c>Task</c> resolves.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// <paramref name="image"/> and/or <paramref name="options"/> are null.
        /// </exception>
        /// <exception cref="ImageNotFoundLocallyException">
        /// The Docker daemon cannot find the image locally, and the <see cref="CreateContainerOptions.PullCondition"/>
        /// option is not set to pull it automatically.
        /// </exception>
        /// <exception cref="ImageNotFoundRemotelyException">
        /// The Docker image does not exist, even remotely. (Only applies when pulling an image, which is not enabled by
        /// default.)
        /// </exception>
        /// <exception cref="NetworkNotFoundException">One of the networks specified does not exist.</exception>
        /// <exception cref="MalformedReferenceException">
        /// The options specified a name for the image, but the name does not meet the expectations of a well-formed
        /// container name.
        /// </exception>
        /// <exception cref="RegistryAuthException">
        /// The registry requires credentials that the client hasn't been given. (Only applies when pulling an image,
        /// which is not enabled by default.)
        /// </exception>
        /// <exception cref="System.Net.Http.HttpRequestException">
        /// The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate
        /// validation, or timeout.
        /// </exception>
        public async Task<IContainer> CreateAndStartAsync(ImageReference image, CreateContainerOptions options, CancellationToken ct = default)
        {
            var container = await CreateAsync(image, options, ct).ConfigureAwait(false);
            await container.StartAsync(ct).ConfigureAwait(false);
            return container;
        }

        /// <summary>
        /// Creates a Docker container from an image.
        /// </summary>
        /// <param name="image">The image to create the container from.</param>
        /// <param name="ct">A token used to cancel the operation.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"><paramref name="image"/> is null or empty.</exception>
        /// <exception cref="ImageNotFoundLocallyException">
        /// The Docker daemon cannot find the image locally, and the <see cref="CreateContainerOptions.PullCondition"/>
        /// option is not set to pull it automatically.
        /// </exception>
        /// <exception cref="System.Net.Http.HttpRequestException">
        /// The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate
        /// validation, or timeout.
        /// </exception>
        /// <exception cref="MalformedReferenceException">
        /// The <paramref name="image"/> input is not a validly-formatted image reference.
        /// </exception>
        public Task<IContainer> CreateAsync(string image, CancellationToken ct = default)
            => CreateAsync(ImageReference.Parse(image), new CreateContainerOptions(), ct);

        /// <summary>
        /// Creates a Docker container from an image.
        /// </summary>
        /// <param name="image">The image to create the container from.</param>
        /// <param name="ct">A token used to cancel the operation.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"><paramref name="image"/> is null.</exception>
        /// <exception cref="ImageNotFoundLocallyException">
        /// The Docker daemon cannot find the image locally, and the <see cref="CreateContainerOptions.PullCondition"/>
        /// option is not set to pull it automatically.
        /// </exception>
        /// <exception cref="System.Net.Http.HttpRequestException">
        /// The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate
        /// validation, or timeout.
        /// </exception>
        public Task<IContainer> CreateAsync(ImageReference image, CancellationToken ct = default)
            => CreateAsync(image, new CreateContainerOptions(), ct);

        /// <summary>
        /// Creates a Docker container from an image.
        /// </summary>
        /// <param name="image">The image to create the container from.</param>
        /// <param name="options">Options for how to create the image and how it should behave.</param>
        /// <param name="ct">A token used to cancel the operation.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="image"/> and/or <paramref name="options"/> are null or empty.
        /// </exception>
        /// <exception cref="ImageNotFoundLocallyException">
        /// The Docker daemon cannot find the image locally, and the <see cref="CreateContainerOptions.PullCondition"/>
        /// option is not set to pull it automatically.
        /// </exception>
        /// <exception cref="ImageNotFoundRemotelyException">
        /// The Docker image does not exist, even remotely. (Only applies when pulling an image, which is not enabled by
        /// default.)
        /// </exception>
        /// <exception cref="MalformedReferenceException">
        /// The image name is not well-formed. --or-- The options specified a name for the image, but the name does not
        /// meet the expectations of a well-formed container name.
        /// </exception>
        /// <exception cref="RegistryAuthException">
        /// The registry requires credentials that the client hasn't been given. (Only applies when pulling an image,
        /// which is not enabled by default.)
        /// </exception>
        /// <exception cref="System.Net.Http.HttpRequestException">
        /// The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate
        /// validation, or timeout.
        /// </exception>
        public Task<IContainer> CreateAsync(string image, CreateContainerOptions options, CancellationToken ct = default)
            => CreateAsync(ImageReference.Parse(image), options, ct);

        /// <summary>
        /// Creates a Docker container from an image.
        /// </summary>
        /// <param name="image">The image to create the container from.</param>
        /// <param name="options">Options for how to create the image and how it should behave.</param>
        /// <param name="ct">A token used to cancel the operation.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="image"/> and/or <paramref name="options"/> are null.
        /// </exception>
        /// <exception cref="ImageNotFoundLocallyException">
        /// The Docker daemon cannot find the image locally, and the <see cref="CreateContainerOptions.PullCondition"/>
        /// option is not set to pull it automatically.
        /// </exception>
        /// <exception cref="ImageNotFoundRemotelyException">
        /// The Docker image does not exist, even remotely. (Only applies when pulling an image, which is not enabled by
        /// default.)
        /// </exception>
        /// <exception cref="MalformedReferenceException">
        /// The options specified a name for the image, but the name does not meet the expectations of a well-formed
        /// container name.
        /// </exception>
        /// <exception cref="RegistryAuthException">
        /// The registry requires credentials that the client hasn't been given. (Only applies when pulling an image,
        /// which is not enabled by default.)
        /// </exception>
        /// <exception cref="System.Net.Http.HttpRequestException">
        /// The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate
        /// validation, or timeout.
        /// </exception>
        public async Task<IContainer> CreateAsync(ImageReference image, CreateContainerOptions options, CancellationToken ct = default)
        {
            if (image is null)
                throw new ArgumentNullException(nameof(image));
            if (options is null)
                throw new ArgumentNullException(nameof(options));

            if (options.Name is not null)
                _ = ContainerName.Parse(options.Name);

            // Optionally pull the image.
            await PullConditionallyAsync(options.PullCondition, image, ct).ConfigureAwait(false);

            // Build the creation request's parameters.
            var request = new CoreModels.CreateContainerParameters
            {
                Image = image,
                Name = options.Name,
                Hostname = options.Hostname,
                Domainname = options.DomainName,
                User = options.User,
                HostConfig = new CoreModels.HostConfig
                {
                    PortBindings = MakePortBindings(options.PortBindings),
                    Isolation = options.IsolationTech,
                    AutoRemove = options.ExitAction == ContainerExitAction.Remove,
                    RestartPolicy = options.ExitAction switch
                    {
                        ContainerExitAction.Restart => new CoreModels.RestartPolicy { Name = CoreModels.RestartPolicyKind.Always },
                        ContainerExitAction.RestartUnlessStopped => new CoreModels.RestartPolicy { Name = CoreModels.RestartPolicyKind.UnlessStopped },
                        ContainerExitAction.RestartOnFailure => new CoreModels.RestartPolicy { Name = CoreModels.RestartPolicyKind.OnFailure, MaximumRetryCount = options.MaximumRetriesCount ?? 3 },
                        _ => new CoreModels.RestartPolicy { Name = CoreModels.RestartPolicyKind.No }
                    },
                },
                Entrypoint = options.Entrypoint,
                Cmd = options.Command,
                Env = MakeEnvironmentVariables(options.EnvironmentVariables),
                NetworkDisabled = options.DisableNetworking,
                Labels = options.Labels,
                NetworkingConfig = MakeNetworkConfigs(options.Networks),
            };

            // Start capturing creation events. We want to find the event about the new container, but we don't have the
            // container's ID yet. So we store up events behind a "dam" and will release them once we have the ID.
            var found = new TaskCompletionSource<ContainerEvent>();
            string? id = null;
            using var sub = this
                .OfType<ContainerCreatedEvent>()
                .Dam(out Action open)
                .Where(ev => ev.ContainerId == id)
                .Subscribe(ev => found.SetResult(ev));

            // Call the API endpoint.
            CoreModels.CreateContainerResponse response;
            try
            {
                response = await _docker.Comm.Containers.CreateContainerAsync(request, ct).ConfigureAwait(false);
            }
            catch (Core.DockerApiException ex)
            {
                if (ImageNotFoundLocallyException.TryWrap(ex, image, out DockerException? wrapped))
                    throw wrapped;
                throw DockerException.Wrap(ex);
            }

            // Now that we have the ID, start processing events to look for it.
            id = response.ID;
            open();

            var output = new Container(_docker, new ContainerFullId(response.ID));

            // Don't leave the method until we have acknowledgement that the operation has completed *and* all direct
            // observers have been notified.
            ContainerEvent completionEvent = await found.Task.ConfigureAwait(false);
            await completionEvent.Delivered.ConfigureAwait(false);

            return output;
        }

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
        /// <exception cref="ArgumentException"><paramref name="container"/> is null or empty.</exception>
        /// <remarks>
        /// Caution: Container names and short IDs are not guaranteed to be unique. If there is more than one match for
        /// the reference, the result is undefined.
        /// </remarks>
        public Task<IContainer> GetAsync(string container, CancellationToken ct = default)
            => GetAsync(ContainerReference.Parse(container), ct);

        /// <summary>
        /// Loads an object that can be used to interact with the indicated container.
        /// </summary>
        /// <param name="container">A container name or ID.</param>
        /// <param name="ct">A token used to cancel the operation.</param>
        /// <returns>A <see cref="Task{TResult}"/> that resolves to the container object.</returns>
        /// <exception cref="ArgumentException"><paramref name="container"/> is null.</exception>
        /// <exception cref="ContainerNotFoundException">No such container exists.</exception>
        /// <exception cref="System.Net.Http.HttpRequestException">
        /// The request failed due to an underlying issue such as loss of network connectivity.
        /// </exception>
        /// <remarks>
        /// Caution: Container names and short IDs are not guaranteed to be unique. If there is more than one match for
        /// the reference, the result is undefined.
        /// </remarks>
        public async Task<IContainer> GetAsync(ContainerReference container, CancellationToken ct = default)
        {
            if (container is null)
                throw new ArgumentNullException(nameof(container));

            return await ContainerFactory.LoadAsync(_docker, container, ct).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets detailed information about a container.
        /// </summary>
        /// <param name="container">The name or ID of a container.</param>
        /// <param name="ct">A <see cref="CancellationToken"/> used to cancel the operation.</param>
        /// <returns>A <see cref="Task"/> that completes when the result is available.</returns>
        /// <exception cref="System.Net.Http.HttpRequestException">
        /// The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate
        /// validation, or timeout.
        /// </exception>
        /// <exception cref="ContainerNotFoundException">
        /// The daemon doesn't know of a container matching the given ID or name.
        /// </exception>
        /// <exception cref="MalformedReferenceException">
        /// The given reference isn't in a valid format for a container ID or name.
        /// </exception>
        /// <exception cref="ArgumentException"><paramref name="container"/> is null or empty.</exception>
        public Task<IContainerInfo> GetDetailsAsync(string container, CancellationToken ct = default)
            => GetDetailsAsync(ContainerReference.Parse(container), ct);

        /// <summary>
        /// Gets detailed information about a container.
        /// </summary>
        /// <param name="container">The name or ID of a container.</param>
        /// <param name="ct">A <see cref="CancellationToken"/> used to cancel the operation.</param>
        /// <returns>A <see cref="Task"/> that completes when the result is available.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="container"/> is null.</exception>
        /// <exception cref="System.Net.Http.HttpRequestException">
        /// The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate
        /// validation, or timeout.
        /// </exception>
        /// <exception cref="ContainerNotFoundException">
        /// The daemon doesn't know of a container matching the given ID or name.
        /// </exception>
        public async Task<IContainerInfo> GetDetailsAsync(ContainerReference container, CancellationToken ct = default)
        {
            if (container is null)
                throw new ArgumentNullException(nameof(container));

            return await ContainerFactory.LoadInfoAsync(_docker, container, ct);
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
        public Task<IReadOnlyList<IContainer>> ListAsync(CancellationToken ct = default)
            => ListAsync(new ListContainersOptions(), ct);

        /// <summary>
        /// Gets a list of Docker containers known to the daemon.
        /// </summary>
        /// <param name="options">Filters for the search.</param>
        /// <param name="ct">A token used to cancel the operation.</param>
        /// <returns>A <see cref="Task{TResult}"/> that resolves to the list of containers.</returns>
        /// <remarks>
        /// <exception cref="ArgumentNullException"><paramref name="options"/> is null.</exception>
        /// <para>
        /// This method does not necessarily return the same results as the `docker image ls` command. To get the same
        /// results, pass in <see cref="ListContainersOptions.CommandLineDefaults"/>.
        /// </para>
        /// <para>The sequence of the results is undefined.</para>
        /// </remarks>
        public async Task<IReadOnlyList<IContainer>> ListAsync(ListContainersOptions options, CancellationToken ct = default)
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
                response = await _docker.Comm.Containers.ListContainersAsync(request, ct).ConfigureAwait(false);
            }
            catch (Core.DockerApiException ex)
            {
                throw DockerException.Wrap(ex);
            }

            return response
                .Select(r => new Container(_docker, new(r.ID)))
                .ToImmutableArray();
        }

        /// <summary>
        /// Starts a Docker container, if it is not already running.
        /// </summary>
        /// <param name="container">The container to start.</param>
        /// <param name="ct">A token used to cancel the operation.</param>
        /// <returns>A <see cref="Task{TResult}"/> that resolves when the container's main process has started.</returns>
        /// <remarks>
        /// <para>
        /// From the perspective of Docker, there's no concept of whether the container's main process has "finished
        /// starting"--just that the process has been started at all. Thus, for example, if the process is a web server,
        /// this method's <c>Task</c> may resolve before the web server is ready for connections. If the application
        /// using this library needs to synchronize with events happening inside the container, it should monitor the
        /// container's logs or use other real-time mechanisms to do so.
        /// </para>
        /// <para>
        /// It's also possible that a short-lived process might exit before the method's <c>Task</c> resolves.
        /// </para>
        /// </remarks>
        /// <exception cref="ContainerNotFoundException">The indicated container does not exist.</exception>
        /// <exception cref="NetworkNotFoundException">One of the networks specified during container creation does not exist.</exception>
        /// <exception cref="System.Net.Http.HttpRequestException">
        /// The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate
        /// validation, or timeout.
        /// </exception>
        /// <exception cref="MalformedReferenceException">
        /// The <paramref name="container"/> input is not a well-formed container reference.
        /// </exception>
        /// <exception cref="ArgumentException"><paramref name="container"/> is null or empty.</exception>
        public Task StartAsync(string container, CancellationToken ct = default)
            => StartAsync(ContainerReference.Parse(container), ct);

        /// <summary>
        /// Starts a Docker container, if it is not already running.
        /// </summary>
        /// <param name="container">The container to start.</param>
        /// <param name="ct">A token used to cancel the operation.</param>
        /// <returns>A <see cref="Task{TResult}"/> that resolves when the container's main process has started.</returns>
        /// <remarks>
        /// <para>
        /// From the perspective of Docker, there's no concept of whether the container's main process has "finished
        /// starting"--just that the process has been started at all. Thus, for example, if the process is a web server,
        /// this method's <c>Task</c> may resolve before the web server is ready for connections. If the application
        /// using this library needs to synchronize with events happening inside the container, it should monitor the
        /// container's logs or use other real-time mechanisms to do so.
        /// </para>
        /// <para>
        /// It's also possible that a short-lived process might exit before the method's <c>Task</c> resolves.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentException"><paramref name="container"/> is null.</exception>
        /// <exception cref="ContainerNotFoundException">The indicated container does not exist.</exception>
        /// <exception cref="NetworkNotFoundException">One of the networks specified during container creation does not exist.</exception>
        /// <exception cref="System.Net.Http.HttpRequestException">
        /// The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate
        /// validation, or timeout.
        /// </exception>
        public async Task StartAsync(ContainerReference container, CancellationToken ct = default)
        {
            // The event match condition needs the full ID, so fetch it if we don't have it already.
            if (container is not ContainerFullId cfid)
                cfid = await ToFullIdAsync(container, ct).ConfigureAwait(false);

            // Start watching for the event that means the operation is done.
            var found = new TaskCompletionSource<ContainerEvent>();
            using var subscription = this
                .OfType<ContainerStartedEvent>()
                .Where(ev => ev.ContainerId == cfid)
                .Subscribe(ev => found.SetResult(ev));

            // Send the request to the API.
            bool alreadyRunning;
            try
            {
                alreadyRunning = !await _docker.Comm.Containers.StartContainerAsync(container, new(), ct).ConfigureAwait(false);
            }
            catch (Core.DockerApiException ex)
            {
                // A bug in Docker.DotNet (https://github.com/dotnet/Docker.DotNet/issues/519) causes a
                // ContainerNotFoundException exception when it should really be a network-not-found exception. Attempt
                // to detect this and return the correct exception. When the bug is fixed, we can change this code to
                // detect network-not-found more properly.
                var match = Regex.Match(ex.ResponseBody, "\"message\":\"network (.*) not found\"");
                if (match.Success)
                {
                    var network = match.Groups[0].Value;
                    throw new NetworkNotFoundException($"No network \"{network}\" exists.");
                }

                if (ContainerNotFoundException.TryWrap(ex, container, out var wrapper))
                    throw wrapper;
                throw DockerException.Wrap(ex);
            }

            // If the container was already running, don't wait for a message about it. (There won't be one.)
            if (alreadyRunning)
                return;

            // Wait until we receive the confirmation message *and* all direct subscribers have been notified.
            var completionEvent = await found.Task.ConfigureAwait(false);
            await completionEvent.Delivered.ConfigureAwait(false);
        }

        /// <summary>
        /// Subscribes to events about containers.
        /// </summary>
        /// <param name="observer">An object to observe the events.</param>
        /// <returns>
        /// An <see cref="IDisposable"/> representing the subscription. Disposing this unsubscribes and releases
        /// resources.
        /// </returns>
        public IDisposable Subscribe(IObserver<ContainerEvent> observer)
            => _docker.OfType<ContainerEvent>().Subscribe(observer);

        internal static IDictionary<string, IList<CoreModels.PortBinding>> MakePortBindings(IEnumerable<PortBinding> portBindings)
        {
            return (from binding in portBindings
                    let key = GetKey(binding)
                    group binding.HostEndpoint by key into g
                    let hostParts = FormatEndpoints(g)
                    select new KeyValuePair<string, IList<CoreModels.PortBinding>>(g.Key, hostParts))
                   .ToDictionary();

            string GetKey(PortBinding binding)
                => binding.ContainerPort.ToStringI()
                + binding.Transport switch
                {
                    TransportType.Udp => "/udp",
                    TransportType.Tcp => "/tcp",
                    _ => ""
                };

            CoreModels.PortBinding[] FormatEndpoints(IEnumerable<IPEndPoint> endpoints)
                => endpoints.Select(FormatEndpoint).ToArray();

            CoreModels.PortBinding FormatEndpoint(IPEndPoint endpoint)
                => new()
                {
                    HostIP = endpoint.Address.ToString(),
                    HostPort = endpoint.Port.ToStringI(),
                };
        }

        private IList<string> MakeEnvironmentVariables(Dictionary<string, string?> environmentVariables)
        {
            if (environmentVariables.Keys.Any(k => k.Contains('=')))
                throw new ArgumentException("Environment variable names must not contain equal signs.");

            return environmentVariables
                   .Select(Convert)
                   .ToArray();

            static string Convert(KeyValuePair<string, string?> kvp) => kvp.Value is null ? kvp.Key : $"{kvp.Key}={kvp.Value}";
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

        private CoreModels.NetworkingConfig? MakeNetworkConfigs(IEnumerable<NetworkReference> networks)
        {
            if (!networks.Any())
                return null;
            var x = networks.ToDictionary(net => net.ToString(), net => new CoreModels.EndpointSettings());
            return new CoreModels.NetworkingConfig
            {
                EndpointsConfig = x,
            };
        }

        private async Task PullConditionallyAsync(PullImageCondition pullCondition, ImageReference image, CancellationToken ct)
        {
            if (pullCondition == PullImageCondition.Never)
                return;

            if (pullCondition == PullImageCondition.Always)
            {
                await _docker.Images.PullAsync(image, ct).ConfigureAwait(false);
                return;
            }

            try
            {
                _ = await _docker.Images.GetAsync(image, ct).ConfigureAwait(false);
                return;  // The image already exists.
            }
            catch (ImageNotFoundLocallyException)
            {
                // The image doesn't already exist, so pull it.
                await _docker.Images.PullAsync(image, ct).ConfigureAwait(false);
                return;
            }
        }

        internal async Task<ContainerFullId> ToFullIdAsync(ContainerReference input, CancellationToken ct = default)
        {
            if (input is ContainerFullId cfid)
                return cfid;

            var container = await GetAsync(input, ct).ConfigureAwait(false);
            return container.Id;
        }

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
