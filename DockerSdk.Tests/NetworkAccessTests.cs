using System;
using System.Net;
using System.Threading.Tasks;
using DockerSdk.Containers;
using DockerSdk.Networks;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace DockerSdk.Tests
{
    [Collection("Common")]
    public class NetworkAccessTests
    {
        public NetworkAccessTests(ITestOutputHelper toh)
        {
            this.toh = toh;
        }

        private readonly ITestOutputHelper toh;

        [Fact]
        public async Task GetAsync_ByFullId_NetworkExists_Success()
        {
            using var client = await DockerClient.StartAsync();
            using var cli = new DockerCli(toh);
            var name = "ddnt-" + nameof(GetAsync_ByFullId_NetworkExists_Success);
            string id = cli.Invoke($"network create {name}")[0];
            try
            {
                Network result = await client.Networks.GetAsync(id);

                result.Id.ToString().Should().Be(id);
            }
            finally
            {
                cli.Invoke($"network rm {id}", ignoreErrors: true);
            }
        }

        [Fact]
        public async Task GetAsync_ByName_NetworkExists_Success()
        {
            using var client = await DockerClient.StartAsync();
            using var cli = new DockerCli(toh);

            // Create a network for testing. To avoid errors where Docker runs out of network
            var name = "ddnt-" + nameof(GetAsync_ByName_NetworkExists_Success);
            string id = cli.Invoke($"network create --subnet 201.101.103.0/24 {name}")[0];  // subnet is to avoid errors from running out of IPs from the default address pool

            try
            {
                Network result = await client.Networks.GetAsync(name);

                result.Id.ToString().Should().Be(id);
            }
            finally
            {
                cli.Invoke($"network rm {name}", ignoreErrors: true);
            }
        }

        [Fact]
        public async Task GetAsync_NetworkDoesNotExist_ThrowsNetworkNotFoundException()
        {
            using var client = await DockerClient.StartAsync();
            using var cli = new DockerCli(toh);
            var name = "ddnt-" + nameof(GetAsync_NetworkDoesNotExist_ThrowsNetworkNotFoundException);

            await Assert.ThrowsAsync<NetworkNotFoundException>(
                () => client.Networks.GetAsync(name));
        }

        [Fact]
        public async Task GetDetailsAsync_NetworkIsAttachedToContainer_GetsEndpointDetails()
        {
            using var client = await DockerClient.StartAsync();
            using var cli = new DockerCli(toh);
            string networkId = cli.GetNetworkId("general");

            // Set up a container and connect to it.
            string containerName = "ddnt-" + nameof(GetDetailsAsync_NetworkIsAttachedToContainer_GetsEndpointDetails);
            string containerIdString = cli.Invoke($"container run --rm --detach --name {containerName} ddnt:infinite-loop")[0];
            cli.Invoke($"network connect --ip 12.34.56.71 --ip6 1234:5678:0000:0000:0000:ff00:0042:8329 {networkId} {containerIdString}");

            ContainerFullId containerIdObject = ContainerFullId.Parse(containerIdString);
            try
            {
                NetworkInfo network = await client.Networks.GetInfoAsync(networkId);

                network.AttachedContainers.Should().HaveCount(1);   
                network.Endpoints.Should().HaveCount(1);
                network.EndpointsByContainerName.Should().HaveCount(1);
                network.EndpointsByContainerName.Keys.Should().Contain(containerIdObject);

                var endpoint = network.EndpointsByContainerName[containerIdObject];
                endpoint.Container.Id.ToString().Should().Be(containerIdString);
                endpoint.Id.Should().NotBeNullOrWhiteSpace();
                endpoint.IPv4Address?.ToString().Should().Be("12.34.56.71");
                endpoint.IPv6Address.Should().Be(IPAddress.Parse("1234:5678::ff00:42:8329"));
                endpoint.MacAddress.Should().NotBeNull();
                endpoint.Network.Id.ToString().Should().Be(networkId);
            }
            finally
            {
                cli.Invoke($"container rm --force {containerIdString}", ignoreErrors: true);
            }
        }

        [Fact]
        public async Task GetDetailsAsync_DetailsAreCorrect()
        {
            using var client = await DockerClient.StartAsync();
            using var cli = new DockerCli(toh);
            string networkId = cli.GetNetworkId("general");

            NetworkInfo network = await client.Networks.GetInfoAsync(networkId);

            network.Labels["ddnt1"].Should().Be("alpha");
            network.Labels["ddnt2"].Should().Be("beta");
            network.CreationTime.Should().BeAfter(DateTimeOffset.Parse("2021-1-1")).And.BeBefore(DateTimeOffset.Now);
            network.Id.ToString().Should().Be(networkId);
            network.IpamDriverName.Should().Be("default");
            network.IsAttachable.Should().Be(true);
            network.IsIngress.Should().Be(false);
            network.IsInternalOnly.Should().Be(false);
            network.IsIPv6Enabled.Should().Be(true);
            network.Name.ToString().Should().Be("ddnt-general");
            network.NetworkDriverName.Should().Be("bridge");
            network.Pools.Should().HaveCount(2);
            network.Pools[0].AuxilliaryAddresses.Should().HaveCount(2);
            network.Pools[0].AuxilliaryAddresses["foo"].ToString().Should().Be("12.34.56.2");
            network.Pools[0].AuxilliaryAddresses["bar"].ToString().Should().Be("12.34.56.3");
            network.Pools[0].Gateway?.ToString().Should().Be("12.34.56.1");
            network.Pools[0].Range?.ToString().Should().Be("12.34.56.0/25");
            network.Pools[0].Subnet.ToString().Should().Be("12.34.56.0/24");
            network.Pools[1].AuxilliaryAddresses.Should().BeEmpty();
            network.Pools[1].Gateway.Should().BeNull();
            network.Pools[1].Range.Should().BeNull();
            network.Pools[1].Subnet.ToString().Should().Be("1234:5678::/32");
            network.Scope.Should().Be(NetworkScope.Local);
        }
    }
}
