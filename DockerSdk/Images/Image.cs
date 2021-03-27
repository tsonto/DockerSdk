using System;
using System.Threading;
using System.Threading.Tasks;

namespace DockerSdk.Images
{
    /// <summary>
    /// Represents a Docker image, which is a read-only template for creating containers.
    /// </summary>
    public class Image
    {
        /// <summary>
        /// Creates an instance of the <see cref="Image"/> class.
        /// </summary>
        /// <param name="client">The <see cref="Docker"/> instance to use.</param>
        /// <param name="id">The image's full ID. If the hash function is not specified, SHA-256 is assumed.S</param>
        internal Image(DockerClient client, string id)
        {
            _client = client;

            if (id.Contains(":"))
                Id = id;
            else
                Id = "sha256:" + id;

            IdShort = ImageAccess.ShortenId(Id);
        }

        /// <summary>
        /// Gets the image's full ID, including the hash function prefix.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Gets the short form of the ID, which is 12 hex digits long and does not include the hash function prefix.
        /// </summary>
        public string IdShort { get; }

        private readonly DockerClient _client;

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
        public Task<ImageDetails> GetDetailsAsync(CancellationToken ct = default)
            => ImageDetails.LoadAsync(_client, Id, ct);

        // TODO: PushAsync
        // TODO: RemoveAsync
        // TODO: ExportAsync (`docker image save` - to stream or file)
        // TODO: TagAsync
    }
}
