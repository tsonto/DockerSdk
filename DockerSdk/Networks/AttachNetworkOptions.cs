using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using DockerSdk.Containers;
using DockerSdk.Core.Models;

namespace DockerSdk.Networks
{
    /// <summary>
    /// Options for how to attach a Docker network to a Docker container.
    /// </summary>
    public class AttachNetworkOptions
    {
        /// <summary>
        /// Gets an editable set of network-scoped aliases to apply to the container.
        /// </summary>
        public ISet<string> ContainerAliases { get; } = new SortedSet<string>();

        /// <summary>
        /// Gets an editable listing of driver options to apply to the the endpoint.
        /// </summary>
        public Dictionary<string, string> DriverOptions { get; } = new();

        /// <summary>
        /// Gets or sets the IPv4 address to use for the container on this network. If this is null, Docker will
        /// automatically assign an IPv4 address from the network's range.
        /// </summary>
        public IPAddress? IPv4Address { get; set; }

        /// <summary>
        /// Gets or sets the IPv6 address to use for the container on this network. If this is null, and the network
        /// supports IPv6, Docker will automatically assign an IPv6 address from the network's range.
        /// </summary>
        public IPAddress? IPv6Address { get; set; }

        internal NetworkConnectParameters ToBodyObject(ContainerReference container)
            => new NetworkConnectParameters
            {
                Container = container,
                EndpointConfig = new EndpointSettings
                {
                    Aliases = ContainerAliases.ToArray(),
                    DriverOpts = DriverOptions,
                    IPAddress = IPv4Address?.ToString(),
                    GlobalIPv6Address = IPv6Address?.ToString(),
                },
            };
    }
}
