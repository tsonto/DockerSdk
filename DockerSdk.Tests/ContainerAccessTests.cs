using System;
using System.Linq;
using System.Threading.Tasks;
using DockerSdk.Containers;
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

        [Fact]
        public async Task GetAsync_StoppedContainer_ReturnsObject()
        {
            // Get the ID of a stopped container.
            var containerId = Cli.Run("docker container ls --filter=ancestor=ddnt:inspect-me-1 --quiet --no-trunc --latest --all").First();

            using var client = await DockerClient.StartAsync();

            var container = await client.Containers.GetAsync(containerId);

            container.Should().NotBeNull();
            container.Id.ToString().Should().Be(containerId);
        }

        [Fact]
        public async Task GetAsync_NoSuchContainer_ThrowsContainerNotFoundException()
        {
            using var client = await DockerClient.StartAsync();

            await Assert.ThrowsAnyAsync<ContainerNotFoundException>(
                () => client.Containers.GetAsync("366ad733152b70e53ddd7fd59defe9fa2e055ed2090f5f3a8839b2797388d0b4"));
        }

        [Fact]
        public async Task GetDetailsAsync_StoppedContainer_NoError_RetrievesCorrectDetails()
        {
            // Get the ID of a stopped container and the image.
            var containerId = Cli.Run("docker container ls --filter=ancestor=ddnt:inspect-me-1 --quiet --no-trunc --latest --all").First();
            var imageId = Cli.Run("docker images ls --no-trunc --quiet --filter reference=ddnt:inspect-me-1").First();

            using var client = await DockerClient.StartAsync();

            var actual = await client.Containers.GetDetailsAsync(containerId);

            actual.Should().NotBeNull();
            actual.Command.Should().Be("/bin/sh");
            actual.CommandArgs.Should().BeEmpty();
            actual.CreationTime.Should().BeBefore(DateTimeOffset.UtcNow).And.BeAfter(DateTimeOffset.Parse("2021-01-01"));
            actual.ErrorMessage.Should().BeNull();
            actual.ExitCode.Should().Be(0);
            actual.Id.ToString().Should().Be(containerId);
            actual.Image.ToString().Should().Be(imageId);
            actual.IsPaused.Should().BeFalse();
            actual.IsRunning.Should().BeFalse();
            actual.IsRunningOrPaused.Should().BeFalse();
            actual.Labels["sample-label-a"].Should().Be("alpha");
            actual.Labels["sample-label-b"].Should().Be("beta");
            actual.MainProcessId.Should().BeNull();
            actual.Name.ToString().Should().NotBeNullOrEmpty();
            actual.RanOutOfMemory.Should().BeNull();
            actual.StartTime.Should().BeBefore(DateTimeOffset.UtcNow).And.BeAfter(actual.CreationTime);
            actual.State.Should().Be(ContainerStatus.Exited);
            actual.StopTime.Should().BeBefore(DateTimeOffset.UtcNow).And.BeAfter(actual.StartTime!.Value);
        }

        [Fact]
        public async Task GetDetailsAsync_RunningContainer_RetrievesCorrectDetails()
        {
            // Get the ID of a running container and the image.
            var containerId = Cli.Run("docker container ls --filter=ancestor=ddnt:infinite-loop --quiet --no-trunc --latest").First();
            var imageId = Cli.Run("docker images ls --no-trunc --quiet --filter reference=ddnt:infinite-loop").First();

            using var client = await DockerClient.StartAsync();

            var actual = await client.Containers.GetDetailsAsync(containerId);

            actual.Should().NotBeNull();
            actual.Command.Should().Be("sleep");
            actual.CommandArgs.Should().BeEquivalentTo("infinity");
            actual.CreationTime.Should().BeBefore(DateTimeOffset.UtcNow).And.BeAfter(DateTimeOffset.Parse("2021-01-01"));
            actual.ErrorMessage.Should().BeNull();
            actual.ExitCode.Should().BeNull();
            actual.Id.ToString().Should().Be(containerId);
            actual.Image.ToString().Should().Be(imageId);
            actual.IsPaused.Should().BeFalse();
            actual.IsRunning.Should().BeTrue();
            actual.IsRunningOrPaused.Should().BeTrue();
            actual.Labels["com.docker.compose.service"].Should().Be("infinite-loop");
            actual.MainProcessId.Should().NotBeNull().And.NotBe(0);
            actual.Name.ToString().Should().NotBeNullOrEmpty();
            actual.RanOutOfMemory.Should().BeNull();
            actual.StartTime.Should().BeBefore(DateTimeOffset.UtcNow).And.BeAfter(actual.CreationTime);
            actual.State.Should().Be(ContainerStatus.Running);
            actual.StopTime.Should().BeNull();
        }

        [Fact]
        public async Task GetDetailsAsync_StoppedContainer_WithError_RetrievesCorrectDetails()
        {
            // Get the ID of a running container and the image.
            var containerId = Cli.Run("docker container ls --filter=ancestor=ddnt:error-out --quiet --no-trunc --latest --all").First();
            var imageId = Cli.Run("docker images ls --no-trunc --quiet --filter reference=ddnt:error-out").First();

            using var client = await DockerClient.StartAsync();

            var actual = await client.Containers.GetDetailsAsync(containerId);

            actual.Should().NotBeNull();
            actual.Command.Should().Be("/bin/sh");
            actual.CommandArgs.Should().BeEquivalentTo("-c", "./script.sh");
            actual.CreationTime.Should().BeBefore(DateTimeOffset.UtcNow).And.BeAfter(DateTimeOffset.Parse("2021-01-01"));
            //actual.ErrorMessage.Should().Be("This is the error message.");    // how to set this?
            actual.ExitCode.Should().Be(1);
            actual.Id.ToString().Should().Be(containerId);
            actual.Image.ToString().Should().Be(imageId);
            actual.IsPaused.Should().BeFalse();
            actual.IsRunning.Should().BeFalse();
            actual.IsRunningOrPaused.Should().BeFalse();
            actual.Labels["com.docker.compose.service"].Should().Be("error-out");
            actual.MainProcessId.Should().BeNull();
            actual.Name.ToString().Should().NotBeNullOrEmpty();
            actual.RanOutOfMemory.Should().BeNull();
            actual.StartTime.Should().BeBefore(DateTimeOffset.UtcNow).And.BeAfter(actual.CreationTime);
            actual.State.Should().Be(ContainerStatus.Exited);
            actual.StopTime.Should().BeBefore(DateTimeOffset.UtcNow).And.BeAfter(actual.StartTime!.Value);
        }
    }
}
