using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace DockerSdk.Tests
{
    [Collection("Common")]
    public class ContainerAccessTests
    {
        [Fact]
        public async Task ListAsync_Defaults_ReturnsStoppedContainer()
        {
            // Get the ID of a stopped container.
            var containerId = Cli.Run("docker container ls --filter=ancestor=ddnt:inspect-me-1 --quiet --no-trunc --latest --all").First();

            using var client = await DockerClient.StartAsync();

            var containers = await client.Containers.ListAsync();

            containers.Should().Contain(container => container.Id == containerId);
        }

        [Fact]
        public async Task ListAsync_ReturnsRunningContainer()
        {
            // Get the ID of a running container.
            var containerId = Cli.Run("docker container ls --filter=ancestor=ddnt:infinite-loop --quiet --no-trunc --latest").First();

            using var client = await DockerClient.StartAsync();

            var containers = await client.Containers.ListAsync();

            containers.Should().Contain(container => container.Id == containerId);
        }
    }
}
