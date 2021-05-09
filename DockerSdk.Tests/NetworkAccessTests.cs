using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
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
                INetwork result = await client.Networks.GetAsync(id);

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

            // Create a network for testing.
            var name = "ddnt-" + nameof(GetAsync_ByName_NetworkExists_Success);
            string id = cli.Invoke($"network create --subnet 201.101.103.0/24 {name}")[0];  // subnet is to avoid errors from running out of IPs from the default address pool

            try
            {
                INetwork result = await client.Networks.GetAsync(name);

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
        public async Task GetDetailsAsync_DetailsAreCorrect()
        {
            using var client = await DockerClient.StartAsync();
            using var cli = new DockerCli(toh);
            string networkId = cli.GetNetworkId("general");

            INetworkInfo network = await client.Networks.GetInfoAsync(networkId);

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

        [Fact]
        public async Task GetDetailsAsync_NetworkIsAttachedToContainer_GetsEndpointDetails()
        {
            using var client = await DockerClient.StartAsync();
            using var cli = new DockerCli(toh);
            string networkId = cli.GetNetworkId("general");

            // Set up a container and connect to it.
            string containerNameString = "ddnt-" + nameof(GetDetailsAsync_NetworkIsAttachedToContainer_GetsEndpointDetails);
            string containerIdString = cli.Invoke($"container run --rm --detach --name {containerNameString} ddnt:infinite-loop")[0];
            cli.Invoke($"network connect --ip 12.34.56.71 --ip6 1234:5678:0000:0000:0000:ff00:0042:8329 {networkId} {containerIdString}");

            ContainerName containerNameObject = ContainerName.Parse(containerNameString);
            try
            {
                INetworkInfo network = await client.Networks.GetInfoAsync(networkId);

                network.AttachedContainers.Should().HaveCount(1);
                network.Endpoints.Should().HaveCount(1);
                network.EndpointsByContainerName.Should().HaveCount(1);
                network.EndpointsByContainerName.Keys.Should().Contain(containerNameObject);

                var endpoint = network.EndpointsByContainerName[containerNameObject];
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
        public async Task ListAsync_FilterByDriver_Success()
        {
            using var client = await DockerClient.StartAsync();
            using var cli = new DockerCli(toh);
            var generalNetworkId = cli.GetNetworkId("general");
            var hostNetworkId = cli.GetHostNetworkId();

            var options = new ListNetworksOptions
            {
                DriverFilter = new[] { "host" },
            };
            var actual = await client.Networks.ListAsync(options);

            actual.Select(net => net.Id.ToString()).Should().NotContain(generalNetworkId);
            actual.Select(net => net.Id.ToString()).Should().Contain(hostNetworkId);
        }

        [Fact]
        public async Task ListAsync_FilterById_Success()
        {
            using var client = await DockerClient.StartAsync();
            using var cli = new DockerCli(toh);
            var generalNetworkId = cli.GetNetworkId("general");
            var hostNetworkId = cli.GetHostNetworkId();

            var options = new ListNetworksOptions
            {
                IdFilter = new[] { generalNetworkId[1..^2] },  // this assumes that the IDs of the two networks differ by more than the first and last digits, which is extremely likely
            };
            var actual = await client.Networks.ListAsync(options);

            actual.Select(net => net.Id.ToString()).Should().Contain(generalNetworkId);
            actual.Select(net => net.Id.ToString()).Should().NotContain(hostNetworkId);
        }

        [Fact]
        public async Task ListAsync_FilterByName_Success()
        {
            using var client = await DockerClient.StartAsync();
            using var cli = new DockerCli(toh);
            var generalNetworkId = cli.GetNetworkId("general");
            var noneNetworkId = cli.GetNoneNetworkId();

            var options = new ListNetworksOptions
            {
                NameFilter = new[] { "-gener" },
            };
            var actual = await client.Networks.ListAsync(options);

            actual.Select(net => net.Id.ToString()).Should().Contain(generalNetworkId);
            actual.Select(net => net.Id.ToString()).Should().NotContain(noneNetworkId);
        }

        [Fact]
        public async Task ListAsync_Success()
        {
            using var client = await DockerClient.StartAsync();
            using var cli = new DockerCli(toh);
            var generalNetworkId = cli.GetNetworkId("general");
            var noneNetworkId = cli.GetNoneNetworkId();
            var hostNetworkId = cli.GetHostNetworkId();

            var actual = await client.Networks.ListAsync();

            actual.Select(net => net.Id.ToString()).Should().Contain(generalNetworkId);
            actual.Select(net => net.Id.ToString()).Should().Contain(noneNetworkId);
            actual.Select(net => net.Id.ToString()).Should().Contain(hostNetworkId);
        }

        [Fact]
        public async Task AttachAsync_ContainerDoesNotExist_ThrowsContainerNotFoundException()
        {
            using var cli = new DockerCli(toh);
            using var client = await DockerClient.StartAsync();
            var network = cli.GetNetwork(client, "ddnt-general");
            
            // Attach a non-existent container. This is the code under test.
            await Assert.ThrowsAsync<ContainerNotFoundException>(
                () => network.AttachAsync("ddnt-no-such-container-exists"));
        }

        [Fact]
        public async Task AttachAsync_ContainerExists_AttachesNetwork()
        {
            using var cli = new DockerCli(toh);
            using var client = await DockerClient.StartAsync();
            var network = cli.GetNetwork(client, "ddnt-general");

            IContainer? container = null;
            try
            {
                // Create and start a container to test with.
                container = cli.CreateAndStartContainer(client);

                // Attach a network. This is the code under test.
                await container.AttachNetwork(network.Id);

                // Check the attachment.
                var json = cli.Invoke("container inspect --format \"{{json .NetworkSettings.Networks}}\" " + container.Id)[0];
                var result = JsonSerializer.Deserialize<Dictionary<string, object>>(json)!.Select(kvp => kvp.Key).ToArray();
                result.Should().Contain("ddnt-general");
            }
            finally
            {
                cli.CleanUpContainer(container);
            }
        }
    }
}
