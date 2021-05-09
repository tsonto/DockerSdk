using System;
using System.Threading;
using System.Threading.Tasks;
using DockerSdk.Containers;
using DockerSdk.Networks;
using DockerSdk.Registries;

namespace DockerSdk.Images
{
    /// <summary>
    /// Represents a Docker image, which is a read-only template for creating containers.
    /// </summary>
    /// <seealso cref="IImageInfo"/>
    public interface IImage
    {
        /// <summary>
        /// Gets the image's full ID.
        /// </summary>
        /// <remarks>
        /// This is a hash of the image's config file. Even with identical build inputs, this will be different for each
        /// build. (Except when you've hacking the process to prevent that.)
        /// </remarks>
        ImageFullId Id { get; }

        /// <summary>
        /// Gets detailed information about the image.
        /// </summary>
        /// <param name="ct">A <see cref="CancellationToken"/> used to cancel the operation.</param>
        /// <returns>A <see cref="Task"/> that completes when the result is available.</returns>
        /// <exception cref="InvalidOperationException">The image no longer exists.</exception>
        /// <exception cref="System.Net.Http.HttpRequestException">
        /// The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate
        /// validation, or timeout.
        /// </exception>
        Task<IImageInfo> GetDetailsAsync(CancellationToken ct = default);

        /// <summary>
        /// Creates a new container from this image and starts it.
        /// </summary>
        /// <param name="ct"></param>
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
        Task<IContainer> RunAsync(CancellationToken ct = default);

        /// <summary>
        /// Creates a new container from this image and starts it.
        /// </summary>
        /// <param name="options">Settings for the new container.</param>
        /// <param name="ct"></param>
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
        /// <paramref name="options"/> is null.
        /// </exception>
        /// <exception cref="ImageNotFoundLocallyException">
        /// The Docker daemon can no longer find the image locally, and the <see cref="CreateContainerOptions.PullCondition"/>
        /// option is not set to pull it automatically.
        /// </exception>
        /// <exception cref="ImageNotFoundRemotelyException">
        /// The Docker image no longer exists, even remotely. (Only applies when pulling an image, which is not enabled by
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
        Task<IContainer> RunAsync(CreateContainerOptions options, CancellationToken ct = default);
    }
}
