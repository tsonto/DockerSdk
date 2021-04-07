using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DockerSdk.Containers
{
    /// <summary>
    /// Represents a Docker container.
    /// </summary>
    public class Container
    {
        private readonly DockerClient _client;

        /// <summary>
        /// Gets the container's full ID.
        /// </summary>
        public ContainerFullId Id { get; }

        internal Container(DockerClient client, ContainerFullId id)
        {
            _client = client;
            Id = id;
        }

        /// <summary>
        /// Gets detailed information about the container.
        /// </summary>
        /// <param name="ct">A <see cref="CancellationToken"/> used to cancel the operation.</param>
        /// <returns>A <see cref="Task"/> that completes when the result is available.</returns>
        /// <exception cref="System.Net.Http.HttpRequestException">
        /// The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate
        /// validation, or timeout.
        /// </exception>
        public Task<ContainerDetails> GetDetailsAsync(CancellationToken ct = default)
            => _client.Containers.GetDetailsAsync(Id, ct);

        // TODO: GetDetailsAsync
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
