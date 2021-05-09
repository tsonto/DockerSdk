using System;
using System.Threading;
using System.Threading.Tasks;
using DockerSdk.Containers.Events;
using DockerSdk.Networks;

namespace DockerSdk.Containers
{
    /// <summary>
    /// Represents a Docker container.
    /// </summary>
    public interface IContainer : IObservable<ContainerEvent>
    {
        /// <summary>
        /// Gets the container's full ID.
        /// </summary>
        ContainerFullId Id { get; }

        /// <summary>
        /// Attaches a Docker network to the container.
        /// </summary>
        /// <param name="network">A reference to the network.</param>
        /// <param name="ct">A token used to cancel the operation.</param>
        /// <returns>A <see cref="Task"/> that resolves when the network has been attached.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="network"/> is null.</exception>
        /// <exception cref="NetworkNotFoundException">The indicated network does not exist.</exception>
        /// <exception cref="ContainerNotFoundException">The container no longer exists.</exception>
        /// <exception cref="System.Net.Http.HttpRequestException">
        /// The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate
        /// validation, or timeout.
        /// </exception>
        Task AttachNetwork(NetworkReference network, CancellationToken ct = default);

        /// <summary>
        /// Attaches a Docker network to the container.
        /// </summary>
        /// <param name="network">A reference to the network.</param>
        /// <param name="options">Options for how to perform the operation.</param>
        /// <param name="ct">A token used to cancel the operation.</param>
        /// <returns>A <see cref="Task"/> that resolves when the network has been attached.</returns>
        /// <exception cref="ArgumentNullException">One or more of the inputs are null.</exception>
        /// <exception cref="ArgumentException">The <paramref name="options"/> input has invalid values, such as an IPv6 address in the <see cref="AttachNetworkOptions.IPv6Address"/> property.</exception>
        /// <exception cref="NetworkNotFoundException">The indicated network does not exist.</exception>
        /// <exception cref="ContainerNotFoundException">The container no longer exists.</exception>
        /// <exception cref="System.Net.Http.HttpRequestException">
        /// The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate
        /// validation, or timeout.
        /// </exception>
        Task AttachNetwork(NetworkReference network, AttachNetworkOptions options, CancellationToken ct = default);

        /// <summary>
        /// Attaches a Docker network to the container.
        /// </summary>
        /// <param name="network">A reference to the network.</param>
        /// <param name="ct">A token used to cancel the operation.</param>
        /// <returns>A <see cref="Task"/> that resolves when the network has been attached.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="network"/> is null.</exception>
        /// <exception cref="NetworkNotFoundException">The indicated network does not exist.</exception>
        /// <exception cref="MalformedReferenceException"><paramref name="network"/> is not a well-formed network reference.</exception>
        /// <exception cref="ContainerNotFoundException">The container no longer exists.</exception>
        /// <exception cref="System.Net.Http.HttpRequestException">
        /// The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate
        /// validation, or timeout.
        /// </exception>
        Task AttachNetwork(string network, CancellationToken ct = default);

        /// <summary>
        /// Attaches a Docker network to the container.
        /// </summary>
        /// <param name="network">A reference to the network.</param>
        /// <param name="options">Options for how to perform the operation.</param>
        /// <param name="ct">A token used to cancel the operation.</param>
        /// <returns>A <see cref="Task"/> that resolves when the network has been attached.</returns>
        /// <exception cref="ArgumentNullException">One or more of the inputs are null.</exception>
        /// <exception cref="ArgumentException">The <paramref name="options"/> input has invalid values, such as an IPv6 address in the <see cref="AttachNetworkOptions.IPv6Address"/> property.</exception>
        /// <exception cref="NetworkNotFoundException">The indicated network does not exist.</exception>
        /// <exception cref="MalformedReferenceException"><paramref name="network"/> is not a well-formed network reference.</exception>
        /// <exception cref="ContainerNotFoundException">The container no longer exists.</exception>
        /// <exception cref="System.Net.Http.HttpRequestException">
        /// The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate
        /// validation, or timeout.
        /// </exception>
        Task AttachNetwork(string network, AttachNetworkOptions options, CancellationToken ct = default);

        /// <summary>
        /// Gets detailed information about the container.
        /// </summary>
        /// <param name="ct">A <see cref="CancellationToken"/> used to cancel the operation.</param>
        /// <returns>A <see cref="Task"/> that completes when the result is available.</returns>
        /// <exception cref="System.Net.Http.HttpRequestException">
        /// The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate
        /// validation, or timeout.
        /// </exception>
        Task<IContainerInfo> GetDetailsAsync(CancellationToken ct = default);

        /// <summary>
        /// Starts the container, if it is not already running.
        /// </summary>
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
        /// <exception cref="ContainerNotFoundException">The indicated container no longer exists.</exception>
        /// <exception cref="NetworkNotFoundException">
        /// One of the networks specified during container creation does not exist.
        /// </exception>
        /// <exception cref="System.Net.Http.HttpRequestException">
        /// The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate
        /// validation, or timeout.
        /// </exception>
        Task StartAsync(CancellationToken ct = default);

        // TODO: ListProcessesAsync
        // TODO: GetLogsAsync
        // TODO: GetFilesystemChangesAsync
        // TODO: ExportAsync
        // TODO: something with the /containers/{id}/stats endpoint?
        // TODO: /containers/{id}/resize endpoint? how useful is that really?
        // TODO: StartAsync  ?useful?
        // TODO: StopAsync  (merge with /containers/{id}/wait as an option)
        // TODO: RestartAsync
        // TODO: KillAsync
        // TODO: ReconfigureAsync
        // TODO: RenameAsync
        // TODO: PauseAsync
        // TODO: UnpauseAsync
        // TODO: attach to container, and websocket variant
        // TODO: RemoveAsync
        // TODO: archive endpoints ?
    }
}
