using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DockerSdk.Images;

using DockerSdk.Core;
using System.Net.Http;
using System.Text.Json.Serialization;
using System.Reactive.Linq;
using System.Reactive.Concurrency;

namespace DockerSdk.Builders
{
    /// <summary>
    /// Provides functionality to build Docker images.
    /// </summary>
    public class Builder
    {
        /// <summary>
        /// Instantiates an instance of the <see cref="Builder"/> class.
        /// </summary>
        /// <param name="client">A connection to the Docker daemon to use.</param>
        public Builder(DockerClient client)
        {
            this.client = client;
        }

        private readonly DockerClient client;

        /// <summary>
        /// Creates a new image from a Dockerfile.
        /// </summary>
        /// <param name="bundle">
        /// A package of the Dockerfile with any other files that need to be available to the build process.
        /// </param>
        /// <param name="options">Specifies how to create the image.</param>
        /// <param name="ct">A token used to cancel the operation.</param>
        /// <returns>
        /// A <see cref="Task{Result}"/> that resolves when the image has been built and is available locally.
        /// </returns>
        /// <exception cref="System.Net.Http.HttpRequestException">
        /// The request failed due to an underlying issue such as loss of network connectivity.
        /// </exception>
        /// <exception cref="DockerImageBuildException">The build failed.</exception>
        public Task<IImage> BuildAsync(IBundle bundle, BuildOptions options, CancellationToken ct = default)
            => BuildAsync(bundle, options, null, ct);

        /// <summary>
        /// Creates a new image from a Dockerfile.
        /// </summary>
        /// <param name="bundle">
        /// A package of the Dockerfile with any other files that need to be available to the build process.
        /// </param>
        /// <param name="options">Specifies how to create the image.</param>
        /// <param name="onProgress">Receives messages about the build process. These match what would display when building from 
        /// the command line. Note that messages may include ANSI escape sequences for color formatting.</param>
        /// <param name="ct">A token used to cancel the operation.</param>
        /// <returns>
        /// A <see cref="Task{Result}"/> that resolves when the image has been built and is available locally.
        /// </returns>
        /// <exception cref="System.Net.Http.HttpRequestException">
        /// The request failed due to an underlying issue such as loss of network connectivity.
        /// </exception>
        /// <exception cref="DockerImageBuildException">The build failed.</exception>
        public async Task<IImage> BuildAsync(IBundle bundle, BuildOptions options, Action<string>? onProgress, CancellationToken ct = default)
        {
            // Get a stream for reading the TAR archive.
            using Stream bundleReader = await bundle.OpenTarForReadAsync().ConfigureAwait(false);

            var queryParameters = options.ToQueryParameters(bundle.DockerfilePath);
            var contentHeaders = new Dictionary<string, string>
            {
                ["Content-Type"] = "application/x-tar",
                // TODO: X-Registry-Config
            };

            // Send the request to the daemon.
            var observable = await client.BuildRequest(HttpMethod.Post, "build")
                .WithParameters(queryParameters)
                .WithBody(bundleReader, contentHeaders)
                .RejectStatus(HttpStatusCode.BadRequest, "dockerfile parse error", err => new DockerImageBuildException($"The build failed: {err}"))
                .SendAndStreamResults<ImageBuildMessage>(ct).ConfigureAwait(false);

            var tcs = new TaskCompletionSource<IImage>();
            //using var ctRegistration = ct.Register(() => tcs.TrySetCanceled()); // TODO: put this back

            using var subscription = observable
                .ObserveOn(ThreadPoolScheduler.Instance)  // avoid race condition when completing the TCS
                .Subscribe(
                item =>
                {
                    // If we have a progress message, emit it.
                    var message = item.Stream;
                    if (message != null)
                        onProgress?.Invoke(message);

                    // Check for an image ID. If we get it, the image must have finished building successfully,
                    // so resolve the task.
                    var id = item.Aux?.ImageId;
                    if (id != null)
                    {
                        var image = new Image(client, new ImageFullId(id));
                        tcs.TrySetResult(image);
                        return;
                    }

                    // Check for an error message. If we get one, the build failed, so throw a build exception.
                    var error = item.Error;
                    if (error != null)
                        tcs.TrySetException(new DockerImageBuildException(error));
                },
                ex => tcs.TrySetException(ex),
                () => tcs.TrySetException(new DockerException("The build stream did not provide an image ID.")));

            return await tcs.Task.ConfigureAwait(false);
        }

        private class ImageBuildMessage
        {
            [JsonPropertyName("aux")]
            public ImageBuildMessageAux? Aux { get; set; }

            [JsonPropertyName("stream")]
            public string? Stream { get; set; }

            [JsonPropertyName("errorDetail")]
            public ImageBuildMessageErrorDetail? ErrorDetail { get; set; }

            [JsonPropertyName("error")]
            public string? Error { get; set; }
        }

        private class ImageBuildMessageAux
        {
            [JsonPropertyName("ID")]
            public string? ImageId { get; set; }
        }

        private class ImageBuildMessageErrorDetail
        {
            [JsonPropertyName("message")]
            public string? Message { get; set; }
        }

        /// <summary>
        /// Creates a new image from a Dockerfile.
        /// </summary>
        /// <param name="contextPath">
        /// The path to the folder containing the Dockerfile and that serves as the context root for files specified in
        /// <paramref name="filePaths"/>. If this is a relative path, it's taken as relative to the process's current
        /// working path.
        /// </param>
        /// <param name="filePaths">
        /// The paths to additional files to make available to the build process. These files must all be within the
        /// context path ( <paramref name="contextPath"/>). Any relative paths are taken as relative to the context
        /// path.
        /// </param>
        /// <param name="options">Specifies how to create the image.</param>
        /// <param name="ct">A token used to cancel the operation.</param>
        /// <returns>
        /// A <see cref="Task{Result}"/> that resolves when the image has been built and is available locally.
        /// </returns>
        /// <exception cref="System.Net.Http.HttpRequestException">
        /// The request failed due to an underlying issue such as loss of network connectivity.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// One or more of the files are not located within the context path.
        /// </exception>
        /// <exception cref="IOException">An I/O error occurred while trying to read one of the files.</exception>
        /// <exception cref="UnauthorizedAccessException">
        /// Either one of the files is a actually directory, or the process does not have permission to access one of
        /// the local files.
        /// </exception>
        /// <exception cref="NotSupportedException">One of the file paths is in an invalid format.</exception>
        /// <exception cref="DockerImageBuildException">The build failed.</exception>
        public async Task<IImage> BuildAsync(string contextPath, IEnumerable<string> filePaths, BuildOptions options, CancellationToken ct = default)
        {
            var bundle = await Bundle.FromFilesAsync(contextPath, filePaths, ct).ConfigureAwait(false);
            return await BuildAsync(bundle, options, ct).ConfigureAwait(false);
        }

        /// <summary>
        /// Creates a new image from a Dockerfile.
        /// </summary>
        /// <param name="contextPath">
        /// The path to the folder that serves as the context root for files specified in <paramref
        /// name="dockerfilePath"/> and <paramref name="filePaths"/>. If this is a relative path, it's taken as relative
        /// to the process's current working path.
        /// </param>
        /// <param name="dockerfilePath">
        /// The path to the Dockerfile. This file must be within the context path ( <paramref name="contextPath"/>). If
        /// this is a relative path, it's taken as relative to the context path.
        /// </param>
        /// <param name="filePaths">
        /// The paths to additional files to make available to the build process. These files must all be within the
        /// context path ( <paramref name="contextPath"/>). Any relative paths are taken as relative to the context
        /// path.
        /// </param>
        /// <param name="options">Specifies how to create the image.</param>
        /// <param name="ct">A token used to cancel the operation.</param>
        /// <returns>
        /// A <see cref="Task{Result}"/> that resolves when the image has been built and is available locally.
        /// </returns>
        /// <exception cref="System.Net.Http.HttpRequestException">
        /// The request failed due to an underlying issue such as loss of network connectivity.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// One or more of the files are not located within the context path.
        /// </exception>
        /// <exception cref="IOException">An I/O error occurred while trying to read one of the files.</exception>
        /// <exception cref="UnauthorizedAccessException">
        /// Either one of the files is a actually directory, or the process does not have permission to access one of
        /// the local files.
        /// </exception>
        /// <exception cref="NotSupportedException">One of the file paths is in an invalid format.</exception>
        /// <exception cref="DockerImageBuildException">The build failed.</exception>
        public async Task<IImage> BuildAsync(string contextPath, string dockerfilePath, IEnumerable<string> filePaths, BuildOptions options, CancellationToken ct = default)
        {
            var bundle = await Bundle.FromFilesAsync(contextPath, dockerfilePath, filePaths, ct).ConfigureAwait(false);
            return await BuildAsync(bundle, options, ct).ConfigureAwait(false);
        }
    }
}
