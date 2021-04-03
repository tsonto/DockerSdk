using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
