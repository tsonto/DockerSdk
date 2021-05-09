using System;
using System.Threading;
using System.Threading.Tasks;
using DockerSdk.Containers;
using DockerSdk.Networks.Events;

namespace DockerSdk.Networks
{
    /// <summary>
    /// Represents a Docker network.
    /// </summary>
    /// <seealso cref="NetworkInfo"/>
    public interface INetwork : IObservable<NetworkEvent>
    {
        /// <summary>
        /// Gets the networks's full ID.
        /// </summary>
        NetworkFullId Id { get; }

        /// <summary>
        /// Attaches the network to a Docker container.
        /// </summary>
        /// <param name="container">A reference to the container.</param>
        /// <param name="ct">A token used to cancel the operation.</param>
        /// <returns>A <see cref="Task"/> that resolves when the network has been attached.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="container"/> is null.</exception>
        /// <exception cref="NetworkNotFoundException">The network no longer exists.</exception>
        /// <exception cref="ContainerNotFoundException">The indicated container does not exist.</exception>
        /// <exception cref="System.Net.Http.HttpRequestException">
        /// The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate
        /// validation, or timeout.
        /// </exception>
        Task AttachAsync(ContainerReference container, CancellationToken ct = default);

        /// <summary>
        /// Attaches the network to a Docker container.
        /// </summary>
        /// <param name="container">A reference to the container.</param>
        /// <param name="options">Options for how to perform the operation.</param>
        /// <param name="ct">A token used to cancel the operation.</param>
        /// <returns>A <see cref="Task"/> that resolves when the network has been attached.</returns>
        /// <exception cref="ArgumentNullException">One or more of the inputs are null.</exception>
        /// <exception cref="ArgumentException">The <paramref name="options"/> input has invalid values, such as an IPv6 address in the <see cref="AttachNetworkOptions.IPv6Address"/> property.</exception>
        /// <exception cref="NetworkNotFoundException">The network no longer exists.</exception>
        /// <exception cref="ContainerNotFoundException">The indicated container does not exist.</exception>
        /// <exception cref="System.Net.Http.HttpRequestException">
        /// The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate
        /// validation, or timeout.
        /// </exception>
        Task AttachAsync(ContainerReference container, AttachNetworkOptions options, CancellationToken ct = default);

        /// <summary>
        /// Attaches the network to a Docker container.
        /// </summary>
        /// <param name="container">A reference to the container.</param>
        /// <param name="ct">A token used to cancel the operation.</param>
        /// <returns>A <see cref="Task"/> that resolves when the network has been attached.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="container"/> is null.</exception>
        /// <exception cref="NetworkNotFoundException">The network no longer exists.</exception>
        /// <exception cref="MalformedReferenceException"><paramref name="container"/> is not a well-formed container reference.</exception>
        /// <exception cref="ContainerNotFoundException">The indicated container does not exist.</exception>
        /// <exception cref="System.Net.Http.HttpRequestException">
        /// The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate
        /// validation, or timeout.
        /// </exception>
        Task AttachAsync(string container, CancellationToken ct = default);

        /// <summary>
        /// Attaches the network to a Docker container.
        /// </summary>
        /// <param name="container">A reference to the container.</param>
        /// <param name="options">Options for how to perform the operation.</param>
        /// <param name="ct">A token used to cancel the operation.</param>
        /// <returns>A <see cref="Task"/> that resolves when the network has been attached.</returns>
        /// <exception cref="ArgumentNullException">One or more of the inputs are null.</exception>
        /// <exception cref="ArgumentException">The <paramref name="options"/> input has invalid values, such as an IPv6 address in the <see cref="AttachNetworkOptions.IPv6Address"/> property.</exception>
        /// <exception cref="NetworkNotFoundException">The network no longer exists.</exception>
        /// <exception cref="MalformedReferenceException"><paramref name="container"/> is not a well-formed container reference.</exception>
        /// <exception cref="ContainerNotFoundException">The indicated container does not exist.</exception>
        /// <exception cref="System.Net.Http.HttpRequestException">
        /// The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate
        /// validation, or timeout.
        /// </exception>
        Task AttachAsync(string container, AttachNetworkOptions options, CancellationToken ct = default);

        /// <summary>
        /// Loads detailed information about the Docker network.
        /// </summary>
        /// <param name="ct">A token used to cancel the operation.</param>
        /// <returns>A <see cref="Task{TResult}"/> that resolves to the details.</returns>
        /// <exception cref="NetworkNotFoundException">The network no longer exists.</exception>
        /// <exception cref="System.Net.Http.HttpRequestException">
        /// The request failed due to an underlying issue such as network connectivity.
        /// </exception>
        Task<INetworkInfo> GetDetailsAsync(CancellationToken ct = default);
    }
}
