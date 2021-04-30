using System.Threading;
using System.Threading.Tasks;

namespace DockerSdk.Networks
{
    /// <summary>
    /// Represents a Docker network.
    /// </summary>
    /// <seealso cref="NetworkDetails"/>
    public sealed class Network
    {
        private readonly DockerClient client;

        internal Network(DockerClient client, NetworkFullId id)
        {
            this.client = client;
            Id = id;
        }

        /// <summary>
        /// Gets the networks's full ID.
        /// </summary>
        public NetworkFullId Id { get; }

        /// <summary>
        /// Loads detailed information about the Docker network.
        /// </summary>
        /// <param name="ct">A token used to cancel the operation.</param>
        /// <returns>A <see cref="Task{TResult}"/> that resolves to the details.</returns>
        /// <exception cref="NetworkNotFoundException">The network no longer exists.</exception>
        /// <exception cref="System.Net.Http.HttpRequestException">
        /// The request failed due to an underlying issue such as network connectivity.
        /// </exception>
        public Task<NetworkDetails> GetDetailsAsync(CancellationToken ct = default)
            => client.Networks.GetDetailsAsync(Id, ct);
    }
}
