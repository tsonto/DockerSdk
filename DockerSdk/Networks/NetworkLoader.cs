using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using DockerSdk.Containers;
using Core = Docker.DotNet;
using CoreModels = Docker.DotNet.Models;

namespace DockerSdk.Networks
{
    internal static class NetworkLoader
    {
        internal static async Task<INetwork> LoadAsync(DockerClient client, NetworkReference reference, NetworkLoadOptions options, LoadContext context, CancellationToken ct)
        {
            // If we know that we already have it, don't load it again.
            if (context.Networks.TryGet(reference, options.IncludeDetails, out var loadedNetwork))
                return loadedNetwork;

            // If we have a full ID and we haven't been asked to load any details, return a minimal object.
            if (!options.IncludeDetails && reference is NetworkFullId nid)
            {
                var minimal = new Network(client, nid);
                context.Networks.Cache(minimal);
                return minimal;
            }

            // Call the Docker API to load the resource.
            CoreModels.NetworkResponse response;
            try
            {
                response = await client.Core.Networks.InspectNetworkAsync(reference, ct).ConfigureAwait(false);
            }
            catch (Core.DockerApiException ex)
            {
                if (NetworkNotFoundException.TryWrap(ex, reference, out var nex))
                    throw nex;
                throw DockerException.Wrap(ex);
            }

            var id = new NetworkFullId(response.ID);

            // Create the majority of the resource.
            var output = new NetworkInfo(client, id)
            {
                CreationTime = response.Created,
                NetworkDriverName = response.Driver,
                IpamDriverName = response.IPAM.Driver,
                IpamDriverOptions = response.IPAM.Options.ToImmutableDictionary(),
                IsAttachable = response.Attachable,
                IsIngress = response.Ingress,
                IsInternalOnly = response.Internal,
                IsIPv6Enabled = response.EnableIPv6,
                Labels = response.Labels.ToImmutableDictionary(),
                Name = new NetworkName(response.Name),
                NetworkDriverOptions = response.Options.ToImmutableDictionary(),
                Pools = response.IPAM.Config.Select(MakePool).ToArray(),
                Scope = response.Scope switch
                {
                    "local" => NetworkScope.Local,
                    "swarm" => NetworkScope.Swarm,
                    "global" => NetworkScope.Swarm,
                    _ => throw new DockerException($"Unrecognized network scope \"{response.Scope}\".")
                },
            };
            
            // Cache the object.
            context.Networks.Cache(output, reference, output.Name);

            // Build the endpoint objects. This may involve loading more information.
            Dictionary<ContainerName, INetworkEndpoint> endpointsByContainerName = new();
            foreach (var kvp in response.Containers)
            {
                var cid = new ContainerFullId(kvp.Key);
                var cname = new ContainerName(kvp.Key);
                INetworkEndpoint ep = await NetworkEndpointLoader.LoadAsync(client, cid, kvp.Value, options.ContainerLoadOptions, context, output, ct).ConfigureAwait(false);
                endpointsByContainerName[cname] = ep;
            }

            // Add the endpoint information.
            output.EndpointsByContainerName = endpointsByContainerName;
            output.Endpoints = endpointsByContainerName.Values.ToImmutableArray();
            output.AttachedContainers = output.Endpoints.Select(ep => ep.Container).ToImmutableArray();

            return output;
        }

        private static ImmutableDictionary<ContainerFullId, INetworkEndpoint> MakeEndpoints(DockerClient client, CoreModels.NetworkResponse raw, Network network)
        {
            var endpoints =
                from kvp in raw.Containers
                let id = new ContainerFullId(kvp.Key)
                let container = new Container(client, id)
                select new NetworkEndpoint(kvp.Value, container, network);

            return endpoints.ImmutableDictionary(endpoint => endpoint.Container.Id);
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
