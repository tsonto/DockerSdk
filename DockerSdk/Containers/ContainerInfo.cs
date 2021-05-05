using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using DockerSdk.Images;
using DockerSdk.Networks;

namespace DockerSdk.Containers
{
    /// <summary>
    /// Holds detailed information about a Docker container.
    /// </summary>
    /// <remarks>This class holds a snapshot in time. Its information is immutable once created.</remarks>
    internal class ContainerInfo : Container, IContainerInfo
    {
        internal ContainerInfo(DockerClient docker, ContainerFullId id, ContainerName name, Image image)
            : base(docker, id)
        {
            Name = name;
            Image = image;
        }

        /// <inheritdoc/>
        public DateTimeOffset CreationTime { get; init; }

        /// <inheritdoc/>
        public string? ErrorMessage { get; init; }

        /// <inheritdoc/>
        public string Executable { get; init; } = "";

        /// <inheritdoc/>
        public IReadOnlyList<string> ExecutableArgs { get; init; } = Array.Empty<string>();

        /// <inheritdoc/>
        public long? ExitCode { get; init; }

        /// <inheritdoc/>
        public IImage Image { get; }

        /// <inheritdoc/>
        public bool IsPaused { get; init; }

        /// <inheritdoc/>
        public bool IsRunning { get; init; }

        /// <inheritdoc/>
        public bool IsRunningOrPaused { get; init; }

        /// <inheritdoc/>
        public IReadOnlyDictionary<string, string> Labels { get; init; } = ImmutableDictionary<string, string>.Empty;

        /// <inheritdoc/>
        public long? MainProcessId { get; init; }

        /// <inheritdoc/>
        public ContainerName Name { get; }

        /// <inheritdoc/>
        public IReadOnlyList<INetworkEndpoint> NetworkEndpoints { get; init; } = ImmutableArray<INetworkEndpoint>.Empty;

        /// <inheritdoc/>
        public IReadOnlyDictionary<NetworkName, INetworkEndpoint> NetworkEndpointsByNetworkName { get; init; } = ImmutableDictionary<NetworkName, INetworkEndpoint>.Empty;

        /// <inheritdoc/>
        public IReadOnlyList<INetwork> Networks { get; init; } = ImmutableArray<INetwork>.Empty;

        /// <inheritdoc/>
        public NetworkSandbox? NetworkSandbox { get; init; }

        /// <inheritdoc/>
        public bool? RanOutOfMemory { get; init; }

        /// <inheritdoc/>
        public DateTimeOffset? StartTime { get; init; }

        /// <inheritdoc/>
        public ContainerStatus State { get; init; }

        /// <inheritdoc/>
        public DateTimeOffset? StopTime { get; init; }

        /*
         * TODO:
         * Mounts
         * SizeRootFs   include image's base size? bytes?
         * SizeRw       bytes?
         * GraphDriver
         * HostConfig
         * ExecIDs
         * AppArmorProfile  what is it? linux only?
         * ProcessLabel
         * MountLabel
         * Platform
         * Driver       fs driver?
         * RestartCount
         * Node
         * LogPath
         * HostsPath
         * HostnamePath
         * ResolvConfPath
         * Config:
         *      StopSignal
         *      OnBuild
         *      Restarting
         *      Dead
         *      Health
         * NetworkSettings
         * */
    }
}
