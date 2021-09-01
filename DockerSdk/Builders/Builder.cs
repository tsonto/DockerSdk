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
using CoreModels = DockerSdk.Core.Models;
using System.Net.Http;
using System.Text.Json.Serialization;

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
        public async Task<IImage> BuildAsync(IBundle bundle, BuildOptions options, CancellationToken ct = default)
        {
            // Get a stream for reading the TAR archive.
            using Stream bundleReader = await bundle.OpenTarForReadAsync().ConfigureAwait(false);

            var queryParameters = options.ToQueryParameters(bundle.DockerfilePath);
            var headers = new Dictionary<string, string>
            {
                ["Content-Type"] = "application/x-tar",
                // TODO: X-Registry-Config
            };

            // Send the request to the daemon.
            var response = await client.BuildRequest(HttpMethod.Post, "build")
                .WithParameters(queryParameters)
                .WithBody(bundleReader)
                .WithHeaders(headers)
                .RejectStatus(HttpStatusCode.BadRequest, "dockerfile parse error", err => new DockerImageBuildException($"The build failed: {err}"))
                .SendAndStreamResults<ImageBuildMessage>(ct).ConfigureAwait(false);

            var tcs = new TaskCompletionSource<IImage>();

            response.Subscribe(
                item =>
                {
                    var id = item.Aux?.ImageId;
                    if (id != null)
                    {
                        var image = new Image(client, new ImageFullId(id));
                        tcs.TrySetResult(image);
                    }
                },
                ex => tcs.TrySetException(ex),
                () => tcs.TrySetException(new DockerException("The build stream did not provide an image ID.")),
                ct);

            return await tcs.Task.ConfigureAwait(false);
        }

        private class ImageBuildMessage
        {
            [JsonPropertyName("aux")]
            public ImageBuildMessageAux? Aux { get; set; }
        }

        private class ImageBuildMessageAux
        {
            [JsonPropertyName("ID")]
            public string? ImageId { get; set; }
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

        private static JsonElement[] ReadMessages(Stream imageStream)
        {
            using var progressReader = new StreamReader(imageStream);
            var lines = progressReader.ReadToEnd();
            return lines.Split('\n')
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Select(line => JsonSerializer.Deserialize<JsonElement>(line))
                .ToArray(); // must force the enumerable to resolve before progressReader is disposed
        }

        //private async Task<Stream> RunAsync(CoreModels.ImageBuildParameters request, Stream bundleReader, CancellationToken ct)
        //{
            
        //}
    }
}
