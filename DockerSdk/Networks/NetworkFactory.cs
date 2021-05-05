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
    internal static class NetworkFactory
    {
        internal static async Task<INetwork> LoadAsync(DockerClient client, NetworkReference reference, CancellationToken ct)
        {
            CoreModels.NetworkResponse raw = await LoadCoreAsync(client, reference, ct).ConfigureAwait(false);
            return new Network(client, new NetworkFullId(raw.ID));
        }

        internal static async Task<INetworkInfo> LoadInfoAsync(DockerClient client, NetworkReference reference, CancellationToken ct)
        {
            CoreModels.NetworkResponse raw = await LoadCoreAsync(client, reference, ct).ConfigureAwait(false);

            var id = new NetworkFullId(raw.ID);
            var network = new Network(client, id);
            var name = new NetworkName(raw.Name);

            // Build the endpoint objects.
            Dictionary<ContainerName, INetworkEndpoint> endpointsByContainerName = new();
            foreach (var kvp in raw.Containers)
            {
                var cid = new ContainerFullId(kvp.Key);
                var cname = new ContainerName(kvp.Value.Name);
                INetworkEndpoint ep = NetworkEndpointFactory.Create(client, cid, kvp.Value, network);
                endpointsByContainerName[cname] = ep;
            }
            var endpoints = endpointsByContainerName.Values.ToImmutableArray();
            var containers = endpoints.Select(ep => ep.Container).ToImmutableArray(); ;

            var output = new NetworkInfo(client, id, name)
            {
                AttachedContainers = containers,
                CreationTime = raw.Created,
                Endpoints = endpoints,
                EndpointsByContainerName = endpointsByContainerName,
                NetworkDriverName = raw.Driver,
                IpamDriverName = raw.IPAM.Driver,
                IpamDriverOptions = raw.IPAM.Options.ToImmutableDictionary(),
                IsAttachable = raw.Attachable,
                IsIngress = raw.Ingress,
                IsInternalOnly = raw.Internal,
                IsIPv6Enabled = raw.EnableIPv6,
                Labels = raw.Labels.ToImmutableDictionary(),
                NetworkDriverOptions = raw.Options.ToImmutableDictionary(),
                Pools = raw.IPAM.Config.Select(MakePool).ToArray(),
                Scope = GetScope(raw.Scope),
            };

            return output;
        }

        private static NetworkScope GetScope(string scopeString)
        {
            return scopeString switch
            {
                "local" => NetworkScope.Local,
                "swarm" => NetworkScope.Swarm,
                "global" => NetworkScope.Swarm,
                _ => throw new DockerException($"Unrecognized network scope \"{scopeString}\".")
            };
        }

        private static async Task<CoreModels.NetworkResponse> LoadCoreAsync(DockerClient client, NetworkReference reference, CancellationToken ct)
        {
            try
            {
                return await client.Core.Networks.InspectNetworkAsync(reference, ct).ConfigureAwait(false);
            }
            catch (Core.DockerApiException ex)
            {
                if (NetworkNotFoundException.TryWrap(ex, reference, out var nex))
                    throw nex;
                throw DockerException.Wrap(ex);
            }
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
