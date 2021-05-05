using System;
using System.Threading;
using System.Threading.Tasks;
using DockerSdk.Containers;

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
        /// <returns>
        /// A <see cref="Task{TResult}"/> that resolves when the container has started. It resolves to the new
        /// container.
        /// </returns>
        /// <remarks>This method does <em>not</em> wait for the container to finish.</remarks>
        Task<IContainer> RunAsync(CancellationToken ct = default);

        /// <summary>
        /// Creates a new container from this image and starts it.
        /// </summary>
        /// <param name="options">Settings for the new container.</param>
        /// <param name="ct"></param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> that resolves when the container has started. It resolves to the new
        /// container.
        /// </returns>
        /// <remarks>This method does <em>not</em> wait for the container to finish.</remarks>
        Task<IContainer> RunAsync(CreateContainerOptions options, CancellationToken ct = default);
    }
}
