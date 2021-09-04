using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using DockerSdk.Networks.Dto;
using DockerSdk.Volumes.Dto;

namespace DockerSdk.Containers.Dto
{
    internal class ContainerInspectResponse
    {
        /// <summary>
        /// The ID of the container
        /// </summary>
        public string Id { get; set; } = null!;

        /// <summary>
        /// The time the container was created
        /// </summary>
        public DateTimeOffset Created { get; set; }

        /// <summary>
        /// The path to the command being run
        /// </summary>
        public string Path { get; set; } = null!;

        /// <summary>
        /// The arguments to the command being run
        /// </summary>
        public IList<string> Args { get; set; } = null!;

        /// <summary>
        /// ContainerState stores container's running state. It's part of ContainerJSONBase and will be returned by the "inspect" command.
        /// </summary>
        public ContainerState State { get; set; } = null!;

        /// <summary>
        /// The container's image ID
        /// </summary>
        public string Image { get; set; } = null!;

        [JsonPropertyName("ResolvConfPath")]
        public string? ResolveConfPath { get; set; }

        public string? HostnamePath { get; set; }
        
        public string? HostsPath { get; set; }

        public string? LogPath { get; set; }

        //public ContainerNode? Node { get; set; } // TODO: obsolete?

        public string Name { get; set; } = null!;

        public long RestartCount { get; set; }

        public string Driver { get; set; } = null!;

        public string? Platform { get; set; }

        public string? MountLabel { get; set; }

        public string? ProcessLabel { get; set; }

        public string? AppArmorProfile { get; set; }

        /// <summary>
        /// IDs of exec instances that are running in the container.
        /// </summary>
        [JsonPropertyName("ExecIDs")]
        public IList<string>? ExecIds { get; set; }

        /// <summary>
        /// Container configuration that depends on the host we are running on
        /// </summary>
        public HostConfig HostConfig { get; set; } = null!;

        /// <summary>
        /// Information about a container's graph driver.
        /// </summary>
        public GraphDriverData GraphDriver { get; set; } = null!;

        /// <summary>
        /// The size of files that have been created or changed by this container.
        /// </summary>
        [JsonPropertyName("SizeRw")]
        public long? SizeRW { get; set; }

        /// <summary>
        /// The total size of all the files in this container.
        /// </summary>
        [JsonPropertyName("SizeRootFs")]
        public long? SizeRootFS { get; set; }

        /// <summary>
        /// A mount point inside a container
        /// </summary>
        public IList<MountPoint>? Mounts { get; set; } = null!;

        /// <summary>
        /// Configuration for a container that is portable between hosts
        /// </summary>
        public ContainerConfig Config { get; set; } = null!;

        /// <summary>
        /// NetworkSettings exposes the network settings in the API
        /// </summary>
        public NetworkSettings NetworkSettings { get; set; } = null!;
    }
}
