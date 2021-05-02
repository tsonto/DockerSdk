using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using DockerSdk.Containers;
using CoreModels = Docker.DotNet.Models;

namespace DockerSdk.Networks
{
    /// <summary>
    /// Provides detailed information about a Docker network.
    /// </summary>
    /// <remarks>This class holds a snapshot in time. Its information is immutable once created.</remarks>
    public sealed class NetworkDetails
    {
        internal NetworkDetails(DockerClient client, CoreModels.NetworkResponse raw)
        {
            Id = new NetworkFullId(raw.ID);

            var network = new Network(client, Id);
            EndpointsByContainerId = MakeEndpoints(client, raw, network);

            AttachedContainers = EndpointsByContainerId.Keys.Select(id => new Container(client, id)).ToArray();
            CreationTime = raw.Created;
            NetworkDriverName = raw.Driver;
            Endpoints = EndpointsByContainerId.Values.ToArray();
            IpamDriverName = raw.IPAM.Driver;
            IpamDriverOptions = raw.IPAM.Options.ToImmutableDictionary();
            IsAttachable = raw.Attachable;
            IsIngress = raw.Ingress;
            IsInternalOnly = raw.Internal;
            IsIPv6Enabled = raw.EnableIPv6;
            Labels = raw.Labels.ToImmutableDictionary();
            Name = new NetworkName(raw.Name);
            NetworkDriverOptions = raw.Options.ToImmutableDictionary();
            Pools = raw.IPAM.Config.Select(MakePool).ToArray();
            Scope = raw.Scope switch
            {
                "local" => NetworkScope.Local,
                "swarm" => NetworkScope.Swarm,
                "global" => NetworkScope.Swarm,
                _ => throw new DockerException($"Unrecognized network scope \"{raw.Scope}\".")
            };
        }

        /// <summary>
        /// Gets the containers that are attached to the network.
        /// </summary>
        /// <seealso cref="Endpoints"/>
        /// <seealso cref="EndpointsByContainerId"/>
        public IReadOnlyList<Container> AttachedContainers { get; }

        /// <summary>
        /// Gets the date and time at which the network was created.
        /// </summary>
        public DateTimeOffset CreationTime { get; }

        /// <summary>
        /// Gets the endpoints that exist for the network. An endpoint defines an attachment point between a network and
        /// a container.
        /// </summary>
        /// <seealso cref="AttachedContainers"/>
        /// <seealso cref="EndpointsByContainerId"/>
        public IReadOnlyList<NetworkEndpoint> Endpoints { get; }

        /// <summary>
        /// Gets a mapping from container IDs to endpoints. An endpoint defines an attachment point between a network
        /// and a container.
        /// </summary>
        /// <seealso cref="Endpoints"/>
        /// <seealso cref="AttachedContainers"/>
        public IReadOnlyDictionary<ContainerFullId, NetworkEndpoint> EndpointsByContainerId { get; }

        /// <summary>
        /// Gets the network's full ID.
        /// </summary>
        public NetworkFullId Id { get; }

        /// <summary>
        /// Gets the type of Docker <a href="https://en.wikipedia.org/wiki/IP_address_management">IPAM</a> driver that
        /// the network uses.
        /// </summary>
        public string IpamDriverName { get; }

        /// <summary>
        /// Gets a list of the options provided to the network's <a
        /// href="https://en.wikipedia.org/wiki/IP_address_management">IPAM</a> driver.
        /// </summary>
        public IReadOnlyDictionary<string, string> IpamDriverOptions { get; }

        /// <summary>
        /// Gets a value indicating whether the network can be attached to a container.
        /// </summary>
        public bool IsAttachable { get; }

        /// <summary>
        /// Gets a value indicating whether the network is a swarm routing-mesh network.
        /// </summary>
        /// <remarks>
        /// There can only be one ingress network at a time. Ingress networks can only be swarm-scoped, and cannot be
        /// attachable.
        /// </remarks>
        public bool IsIngress { get; }

        /// <summary>
        /// Gets a value indicating whether the network is "internal".
        /// </summary>
        /// <remarks>
        /// By default, when you connect a container to a network, Docker also connects it to the Internet. Creating the
        /// network as internal prevents that.
        /// </remarks>
        public bool IsInternalOnly { get; }

        /// <summary>
        /// Gets a value indicating whether <a href="https://en.wikipedia.org/wiki/IPv6">IPv6</a> networking is enabled.
        /// </summary>
        public bool IsIPv6Enabled { get; }

        /// <summary>
        /// Gets the labels and their values that have been applied to the network.
        /// </summary>
        public IReadOnlyDictionary<string, string> Labels { get; }

        /// <summary>
        /// Gets the network's name.
        /// </summary>
        public NetworkName Name { get; }

        /// <summary>
        /// Gets the string identifier for the network driver that operates this network.
        /// </summary>
        /// <remarks>
        /// Built-in network drivers include "bridge", "overlay", "macvlan", and "none". Docker plugins may provide
        /// additional network drivers.
        /// </remarks>
        public string NetworkDriverName { get; }

        /// <summary>
        /// Gets a list of the options provided to the network driver.
        /// </summary>
        public IReadOnlyDictionary<string, string> NetworkDriverOptions { get; }

        /// <summary>
        /// Gets the address pools that the IPAM driver can use when assigning IP addresses.
        /// </summary>
        public IReadOnlyList<NetworkPool> Pools { get; }

        /// <summary>
        /// Gets the network's scope.
        /// </summary>
        public NetworkScope Scope { get; }

        private static Dictionary<ContainerFullId, NetworkEndpoint> MakeEndpoints(DockerClient client, CoreModels.NetworkResponse raw, Network network)
        {
            var endpoints =
                from kvp in raw.Containers
                let id = new ContainerFullId(kvp.Key)
                let container = new Container(client, id)
                select new NetworkEndpoint(kvp.Value, container, network);

            return endpoints.ToDictionary(endpoint => endpoint.Container.Id);
        }

        private static NetworkPool MakePool(CoreModels.IPAMConfig raw)
        {
            var subnet = IPSubnet.Parse(raw.Subnet);

            IPAddress? gateway = null;
            if (!string.IsNullOrEmpty(raw.Gateway))
                gateway = IPAddress.Parse(raw.Gateway);

            IPSubnet? range = null;
            if (!string.IsNullOrEmpty(raw.IPRange))
                range = IPSubnet.Parse(raw.IPRange);

            IReadOnlyDictionary<string, IPAddress> auxAddresses;
            if (raw.AuxAddress is not null)
                auxAddresses = raw.AuxAddress.SelectValues(str => IPAddress.Parse(str)).ToImmutableDictionary();
            else
                auxAddresses = ImmutableDictionary<string, IPAddress>.Empty;

            return new()
            {
                Subnet = subnet,
                Gateway = gateway,
                Range = range,
                AuxilliaryAddresses = auxAddresses,
            };
        }
    }
}
