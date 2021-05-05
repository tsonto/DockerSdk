using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using DockerSdk.Containers;

namespace DockerSdk.Networks
{
    /// <summary>
    /// Provides detailed information about a Docker network.
    /// </summary>
    /// <remarks>This class holds a snapshot in time. Its information is immutable once created.</remarks>
    internal class NetworkInfo : Network, INetworkInfo
    {
        internal NetworkInfo(DockerClient client, NetworkFullId id, NetworkName name)
            : base(client, id)
        {
            Name = name;
        }

        /// <inheritdoc/>
        public IReadOnlyList<IContainer> AttachedContainers { get; init; } = ImmutableList<IContainer>.Empty;

        /// <inheritdoc/>
        public DateTimeOffset CreationTime { get; init; }

        public IReadOnlyList<INetworkEndpoint> Endpoints { get; set; } = ImmutableList<INetworkEndpoint>.Empty;

        /// <inheritdoc/>
        public IReadOnlyDictionary<ContainerName, INetworkEndpoint> EndpointsByContainerName { get; init; } = ImmutableDictionary<ContainerName, INetworkEndpoint>.Empty;

        /// <inheritdoc/>
        public string IpamDriverName { get; init; } = "";

        /// <inheritdoc/>
        public IReadOnlyDictionary<string, string> IpamDriverOptions { get; init; } = ImmutableDictionary<string, string>.Empty;

        /// <inheritdoc/>
        public bool IsAttachable { get; init; }

        /// <inheritdoc/>
        public bool IsIngress { get; init; }

        /// <inheritdoc/>
        public bool IsInternalOnly { get; init; }

        /// <inheritdoc/>
        public bool IsIPv6Enabled { get; init; }

        /// <inheritdoc/>
        public IReadOnlyDictionary<string, string> Labels { get; init; } = ImmutableDictionary<string, string>.Empty;

        /// <inheritdoc/>
        public NetworkName Name { get; }

        /// <inheritdoc/>
        public string NetworkDriverName { get; init; } = "";

        /// <inheritdoc/>
        public IReadOnlyDictionary<string, string> NetworkDriverOptions { get; init; } = ImmutableDictionary<string, string>.Empty;

        /// <inheritdoc/>
        public IReadOnlyList<NetworkPool> Pools { get; init; } = ImmutableList<NetworkPool>.Empty;

        /// <inheritdoc/>
        public NetworkScope Scope { get; init; }
    }
}
