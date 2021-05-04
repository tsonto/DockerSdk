using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DockerSdk.Containers;
using CoreModels = Docker.DotNet.Models;

namespace DockerSdk.Networks
{
    internal static class NetworkEndpointLoader
    {
        public static async Task<INetworkEndpoint> LoadAsync(DockerClient docker, CoreModels.EndpointSettings raw, NetworkLoadOptions networkLoadOptions, LoadContext context, IContainer container, CancellationToken ct)
        {
            // If we've already loaded them, we don't need to do anything more.
            var id = raw.EndpointID;
            if (context.NetworkEndpoints.TryGetValue(id, out INetworkEndpoint? loadedEndpoint))
                return loadedEndpoint;

            // Create the object with the basic info.
            var ep = new NetworkEndpoint
            {
                Id = id,
                IPv4Address = TryParseIP(raw.IPAddress),
                IPv6Address = TryParseIP(raw.GlobalIPv6Address),
                Container = container,
                MacAddress = PhysicalAddress.Parse(raw.MacAddress),
            };

            // Cache what we have so far, since we might recurse back to this point later.
            context.NetworkEndpoints[id] = ep;

            // Load the network.
            ep.Network = await NetworkLoader.LoadAsync(docker, new NetworkFullId(raw.NetworkID), networkLoadOptions, context, ct).ConfigureAwait(false);

            return ep;
        }

        /// <summary>
        /// Build the object based on what we get from an inspect-network response. (Ironically, we get less network
        /// information from that than with an inspect-container response.)
        /// </summary>
        public static async Task<INetworkEndpoint> LoadAsync(DockerClient docker, ContainerFullId containerId, CoreModels.EndpointResource raw, ContainerLoadOptions containerLoadOptions, LoadContext context, INetwork network, CancellationToken ct)
        {
            // If we've already loaded them, we don't need to do anything more.
            var id = raw.EndpointID;
            if (context.NetworkEndpoints.TryGetValue(id, out INetworkEndpoint? loadedEndpoint))
                return loadedEndpoint;

            // Create the object with the basic info.
            var ep = new NetworkEndpoint
            {
                Id = id,
                IPv4Address = TryParseIP(raw.IPv4Address),
                IPv6Address = TryParseIP(raw.IPv6Address),
                Network = network,
                MacAddress = PhysicalAddress.Parse(raw.MacAddress),
            };

            // Cache what we have so far, since we might recurse back to this point later.
            context.NetworkEndpoints[id] = ep;

            // Load the container.
            ep.Container = await ContainerLoader.LoadAsync(docker, containerId, containerLoadOptions, context, ct).ConfigureAwait(false);

            return ep;

        }

        private static IPAddress? TryParseIP(string? input)
            => string.IsNullOrEmpty(input)
            ? null
            : IPAddress.Parse(input);
    }
}
