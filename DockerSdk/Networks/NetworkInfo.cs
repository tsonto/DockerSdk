using System;
using System.Collections.Generic;
using DockerSdk.Containers;

namespace DockerSdk.Networks
{
    /// <summary>
    /// Provides detailed information about a Docker network.
    /// </summary>
    /// <remarks>This class holds a snapshot in time. Its information is immutable once created.</remarks>
    internal class NetworkInfo : Network, INetworkInfo
    {
        internal NetworkInfo(DockerClient client, NetworkFullId id)
            : base(client, id)
        {
        }

        /// <inheritdoc/>
        public IReadOnlyList<IContainer> AttachedContainers { get; internal set; }

        /// <inheritdoc/>
        public DateTimeOffset CreationTime { get; internal init; }

        public IReadOnlyList<INetworkEndpoint> Endpoints { get; set; }

        /// <inheritdoc/>
        public IReadOnlyDictionary<ContainerName, INetworkEndpoint> EndpointsByContainerName { get; internal set; }

        /// <inheritdoc/>
        public string IpamDriverName { get; internal init; }

        /// <inheritdoc/>
        public IReadOnlyDictionary<string, string> IpamDriverOptions { get; internal init; }

        /// <inheritdoc/>
        public bool IsAttachable { get; internal init; }

        /// <inheritdoc/>
        public bool IsIngress { get; internal init; }

        /// <inheritdoc/>
        public bool IsInternalOnly { get; internal init; }

        /// <inheritdoc/>
        public bool IsIPv6Enabled { get; internal init; }

        /// <inheritdoc/>
        public IReadOnlyDictionary<string, string> Labels { get; internal init; }

        /// <inheritdoc/>
        public NetworkName Name { get; internal init; }

        /// <inheritdoc/>
        public string NetworkDriverName { get; internal init; }

        /// <inheritdoc/>
        public IReadOnlyDictionary<string, string> NetworkDriverOptions { get; internal init; }

        /// <inheritdoc/>
        public IReadOnlyList<NetworkPool> Pools { get; internal init; }

        /// <inheritdoc/>
        public NetworkScope Scope { get; internal init; }
    }
}
