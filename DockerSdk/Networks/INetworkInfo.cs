using System;
using System.Collections.Generic;
using DockerSdk.Containers;

namespace DockerSdk.Networks
{
    /// <summary>
    /// Provides detailed information about a Docker network.
    /// </summary>
    /// <remarks>This class holds a snapshot in time. Its information is immutable once created.</remarks>
    public interface INetworkInfo : INetwork
    {
        /// <summary>
        /// Gets the containers that are attached to the network.
        /// </summary>
        /// <seealso cref="Endpoints"/>
        /// <seealso cref="EndpointsByContainerName"/>
        IReadOnlyList<IContainer> AttachedContainers { get; }

        /// <summary>
        /// Gets the date and time at which the network was created.
        /// </summary>
        DateTimeOffset CreationTime { get; }

        /// <summary>
        /// Gets the endpoints that exist for the network. An endpoint defines an attachment point between a network and
        /// a container.
        /// </summary>
        /// <seealso cref="AttachedContainers"/>
        /// <seealso cref="EndpointsByContainerName"/>
        IReadOnlyList<INetworkEndpoint> Endpoints { get; }

        /// <summary>
        /// Gets a mapping from container names to endpoints. An endpoint defines an attachment point between a network
        /// and a container.
        /// </summary>
        /// <seealso cref="Endpoints"/>
        /// <seealso cref="AttachedContainers"/>
        IReadOnlyDictionary<ContainerName, INetworkEndpoint> EndpointsByContainerName { get; }

        /// <summary>
        /// Gets the type of Docker <a href="https://en.wikipedia.org/wiki/IP_address_management">IPAM</a> driver that
        /// the network uses.
        /// </summary>
        string IpamDriverName { get; }

        /// <summary>
        /// Gets a list of the options provided to the network's <a
        /// href="https://en.wikipedia.org/wiki/IP_address_management">IPAM</a> driver.
        /// </summary>
        IReadOnlyDictionary<string, string> IpamDriverOptions { get; }

        /// <summary>
        /// Gets a value indicating whether the network can be attached to a container.
        /// </summary>
        bool IsAttachable { get; }

        /// <summary>
        /// Gets a value indicating whether the network is a swarm routing-mesh network.
        /// </summary>
        /// <remarks>
        /// There can only be one ingress network at a time. Ingress networks can only be swarm-scoped, and cannot be
        /// attachable.
        /// </remarks>
        bool IsIngress { get; }

        /// <summary>
        /// Gets a value indicating whether the network is "internal".
        /// </summary>
        /// <remarks>
        /// By default, when you connect a container to a network, Docker also connects it to the Internet. Creating the
        /// network as internal prevents that.
        /// </remarks>
        bool IsInternalOnly { get; }

        /// <summary>
        /// Gets a value indicating whether <a href="https://en.wikipedia.org/wiki/IPv6">IPv6</a> networking is enabled.
        /// </summary>
        bool IsIPv6Enabled { get; }

        /// <summary>
        /// Gets the labels and their values that have been applied to the network.
        /// </summary>
        IReadOnlyDictionary<string, string> Labels { get; }

        /// <summary>
        /// Gets the network's name.
        /// </summary>
        NetworkName Name { get; }

        /// <summary>
        /// Gets the string identifier for the network driver that operates this network.
        /// </summary>
        /// <remarks>
        /// Built-in network drivers include "bridge", "overlay", "macvlan", and "none". Docker plugins may provide
        /// additional network drivers.
        /// </remarks>
        string NetworkDriverName { get; }

        /// <summary>
        /// Gets a list of the options provided to the network driver.
        /// </summary>
        IReadOnlyDictionary<string, string> NetworkDriverOptions { get; }

        /// <summary>
        /// Gets the address pools that the IPAM driver can use when assigning IP addresses.
        /// </summary>
        IReadOnlyList<NetworkPool> Pools { get; }

        /// <summary>
        /// Gets the network's scope.
        /// </summary>
        NetworkScope Scope { get; }
    }
}
