using System.Threading;
using System.Threading.Tasks;

namespace DockerSdk.Networks
{
    /// <summary>
    /// Represents a Docker network.
    /// </summary>
    /// <seealso cref="NetworkInfo"/>
    internal class Network : INetwork
    {
        internal Network(DockerClient client, NetworkFullId id)
        {
            this.client = client;
            Id = id;
        }

        /// <inheritdoc/>
        public NetworkFullId Id { get; }

        private readonly DockerClient client;

        /// <inheritdoc/>
        public Task<INetworkInfo> GetDetailsAsync(CancellationToken ct = default)
            => this is INetworkInfo info
            ? Task.FromResult(info)
            : client.Networks.GetInfoAsync(Id, ct);
    }
}
