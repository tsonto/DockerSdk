using System;
using System.Linq;
using System.Threading.Tasks;
using DockerSdk.Containers;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace DockerSdk.Tests
{
    [Collection("Common")]
    public class ContainerAccessTests
    {
        private readonly ITestOutputHelper toh;

        public ContainerAccessTests(ITestOutputHelper toh)
        {
            this.toh = toh;
        }

        [Fact]
        public async Task GetAsync_NoSuchContainer_ThrowsContainerNotFoundException()
        {
            using var client = await DockerClient.StartAsync();

            await Assert.ThrowsAnyAsync<ContainerNotFoundException>(
                () => client.Containers.GetAsync("366ad733152b70e53ddd7fd59defe9fa2e055ed2090f5f3a8839b2797388d0b4"));
        }

        [Fact]
        public async Task GetAsync_StoppedContainer_ReturnsObject()
        {
            // Get the ID of a stopped container.
            using var cli = new DockerCli(toh);
            var containerId = cli.GetContainerId("ddnt-exited");

            using var client = await DockerClient.StartAsync();

            var container = await client.Containers.GetAsync(containerId);

            container.Should().NotBeNull();
            container.Id.ToString().Should().Be(containerId);
        }

        [Fact]
        public async Task GetDetailsAsync_RunningContainer_RetrievesCorrectDetails()
        {
            // Get the ID of a running container and its image.
            using var cli = new DockerCli(toh);
            var containerId = cli.GetContainerId("ddnt-running");
            var imageId = cli.GetImageId("ddnt:infinite-loop");

            using var client = await DockerClient.StartAsync();

            var actual = await client.Containers.GetDetailsAsync(containerId);

            actual.Should().NotBeNull();
            actual.Executable.Should().Be("sleep");
            actual.ExecutableArgs.Should().BeEquivalentTo("infinity");
            actual.CreationTime.Should().BeBefore(DateTimeOffset.UtcNow).And.BeAfter(DateTimeOffset.Parse("2021-01-01"));
            toh.WriteLine("**Error " + actual.ErrorMessage);
            actual.ErrorMessage.Should().BeNull();
            toh.WriteLine("**ExitCode " + actual.ExitCode);
            actual.ExitCode.Should().BeNull();
            actual.Id.ToString().Should().Be(containerId);
            actual.Image.ToString().Should().Be(imageId);
            actual.IsPaused.Should().BeFalse();
            actual.IsRunning.Should().BeTrue();
            actual.IsRunningOrPaused.Should().BeTrue();
            actual.Labels["override-1"].Should().NotBeNullOrEmpty();
            actual.MainProcessId.Should().NotBeNull().And.NotBe(0);
            actual.Name.ToString().Should().NotBeNullOrEmpty();
            actual.RanOutOfMemory.Should().BeNull();
            actual.StartTime.Should().BeBefore(DateTimeOffset.UtcNow).And.BeAfter(actual.CreationTime);
            actual.State.Should().Be(ContainerStatus.Running);
            actual.StopTime.Should().BeNull();
        }

        [Fact]
        public async Task GetDetailsAsync_StoppedContainer_NoError_RetrievesCorrectDetails()
        {
            // Get the ID of a stopped container and its image.
            using var cli = new DockerCli(toh);
            var containerId = cli.GetContainerId("ddnt-exited");
            var imageId = cli.GetImageId("ddnt:inspect-me-1");
            using var client = await DockerClient.StartAsync();

            var actual = await client.Containers.GetDetailsAsync(containerId);

            actual.Should().NotBeNull();
            actual.Executable.Should().Be("/bin/sh");
            actual.ExecutableArgs.Should().BeEmpty();
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
        public async Task GetDetailsAsync_RunLabelsOverrideImageLabels()
        {
            // Get the ID of a stopped container and its image.
            using var cli = new DockerCli(toh);
            var containerId = cli.Run("ddnt:inspect-me-1", args: "--label override-2=\"from-run\"");
            using var client = await DockerClient.StartAsync();

            var actual = await client.Containers.GetDetailsAsync(containerId);

            actual.Labels["override-2"].Should().Be("from-run");
        }

        [Fact]
        public async Task GetDetailsAsync_StoppedContainer_WithError_RetrievesCorrectDetails()
        {
            // Get the ID of a failed container and its image.
            using var cli = new DockerCli(toh);
            var containerId = cli.GetContainerId("ddnt-failed");
            var imageId = cli.GetImageId("ddnt:error-out");
            using var client = await DockerClient.StartAsync();

            var actual = await client.Containers.GetDetailsAsync(containerId);

            actual.Should().NotBeNull();
            actual.Executable.Should().Be("/bin/sh");
            actual.ExecutableArgs.Should().BeEquivalentTo("-c", "/script.sh");
            actual.CreationTime.Should().BeBefore(DateTimeOffset.UtcNow).And.BeAfter(DateTimeOffset.Parse("2021-01-01"));
            //actual.ErrorMessage.Should().Be("This is the error message.");    // how to set this?
            actual.ExitCode.Should().BeOneOf(1, 126);   // The exit code differs on local tests vs CI. Not sure why that is, but the important things is that, whatever the exit code is, we report it correctly.
            actual.Id.ToString().Should().Be(containerId);
            actual.Image.ToString().Should().Be(imageId);
            actual.IsPaused.Should().BeFalse();
            actual.IsRunning.Should().BeFalse();
            actual.IsRunningOrPaused.Should().BeFalse();
            actual.Labels["override-1"].Should().NotBeNullOrEmpty();
            actual.MainProcessId.Should().BeNull();
            actual.Name.ToString().Should().NotBeNullOrEmpty();
            actual.RanOutOfMemory.Should().BeNull();
            actual.StartTime.Should().BeBefore(DateTimeOffset.UtcNow).And.BeAfter(actual.CreationTime);
            actual.State.Should().Be(ContainerStatus.Exited);
            actual.StopTime.Should().BeBefore(DateTimeOffset.UtcNow).And.BeAfter(actual.StartTime!.Value);
        }

        [Fact]
        public async Task ListAsync_Defaults_ReturnsStoppedContainer()
        {
            using var cli = new DockerCli(toh);
            var containerId = cli.GetContainerId("ddnt-exited");
            using var client = await DockerClient.StartAsync();

            var containers = await client.Containers.ListAsync();

            containers.Should().Contain(container => container.Id == containerId);
        }

        [Fact]
        public async Task ListAsync_ReturnsRunningContainer()
        {
            using var cli = new DockerCli(toh);
            var containerId = cli.GetContainerId("ddnt-running");
            using var client = await DockerClient.StartAsync();

            var containers = await client.Containers.ListAsync();

            containers.Should().Contain(container => container.Id == containerId);
        }
    }
}
