using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using DockerSdk.Containers;
using DockerSdk.Containers.Events;
using DockerSdk.Images;
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
            actual.ExecutableArgs.Should().BeEquivalentTo("1000d");
            actual.CreationTime.Should().BeBefore(DateTimeOffset.UtcNow).And.BeAfter(DateTimeOffset.Parse("2021-01-01"));
            actual.ErrorMessage.Should().BeNull();
            toh.WriteLine("**ExitCode " + actual.ExitCode);
            actual.ExitCode.Should().BeNull();
            actual.Id.ToString().Should().Be(containerId);
            actual.Image.Id.ToString().Should().Be(imageId);
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
            actual.Image.Id.ToString().Should().Be(imageId);
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
            actual.StopTime.Should().BeBefore(DateTimeOffset.UtcNow).And.BeAfter(actual.CreationTime); // Don't check that StopTime >= StartTime, because I've observed that being false for this test, even according to `docker inspect`.
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
            actual.Image.Id.ToString().Should().Be(imageId);
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

        [Fact]
        public async Task CreateAsync_CreatesContainer()
        {
            using var cli = new DockerCli(toh);
            using var client = await DockerClient.StartAsync();

            IContainer? container = null;
            try
            {
                container = await client.Containers.CreateAsync("ddnt:infinite-loop");

                var output = cli.Invoke("container ls --all --quiet --no-trunc");
                output.Should().Contain(container.Id.ToString());
            }
            finally
            {
                if (container is not null)
                    cli.Invoke("container rm --force " + container.Id, ignoreErrors: true);
            }
        }

        [Fact]
        public async Task CreateAsync_DoesNotResolveUntilDone()
        {
            using var cli = new DockerCli(toh);
            using var client = await DockerClient.StartAsync();
            var name = "ddnt-" + nameof(CreateAsync_DoesNotResolveUntilDone);
            
            bool found = false;
            using var subscription = client.Containers.Where(e => name == e.ContainerName!).Subscribe(e => found = true);

            IContainer? container = null;
            try
            {
                var request = new CreateContainerOptions { Name = name };
                container = await client.Containers.CreateAsync("ddnt:infinite-loop", request);
                Assert.True(found);
            }
            finally
            {
                if (container is not null)
                    cli.Invoke("container rm --force " + container.Id, ignoreErrors: true);
            }
        }

        [Fact]
        public async Task CreateAsync_SpecifyName_HasThatName()
        {
            using var cli = new DockerCli(toh);
            using var client = await DockerClient.StartAsync();
            const string name = "ddnt-" + nameof(CreateAsync_SpecifyName_HasThatName);

            IContainer? container = null;
            try
            {
                var options = new CreateContainerOptions { Name = name };
                container = await client.Containers.CreateAsync("ddnt:infinite-loop", options);

                var output = cli.Invoke("container ls --all --format=\"{{.ID}}={{.Names}}\" --no-trunc ");
                output.Should().Contain($"{container.Id}={name}");
            }
            finally
            {
                if (container is not null)
                    cli.Invoke("container rm --force " + container.Id, ignoreErrors: true);
            }
        }

        [Fact]
        public async Task CreateAsync_WithLabels_SetsLabels()
        {
            using var cli = new DockerCli(toh);
            using var client = await DockerClient.StartAsync();

            IContainer? container = null;
            try
            {
                var options = new CreateContainerOptions 
                { 
                    Labels =
                    {
                        ["abc"] = "123",
                        ["override-1"] = "from-creation",
                    }
                };
                container = await client.Containers.CreateAsync("ddnt:inspect-me-1", options);

                var output = cli.Invoke("container inspect --format=\"{{json .Config.Labels}}\" " + container.Id)[0];
                output.Should().Contain("\"abc\":\"123\"");
                output.Should().Contain("\"override-1\":\"from-creation\"");
                output.Should().Contain("\"override-2\":\"from-image\"");
            }
            finally
            {
                if (container is not null)
                    cli.Invoke("container rm --force " + container.Id, ignoreErrors: true);
            }
        }

        [Fact]
        public async Task CreateAsync_WithEnvironmentVariables_SetsEnvironmentVariables()
        {
            using var cli = new DockerCli(toh);
            using var client = await DockerClient.StartAsync();

            IContainer? container = null;
            try
            {
                var options = new CreateContainerOptions
                {
                    EnvironmentVariables =
                    {
                        ["abc"] = "123",
                        ["override1"] = "from-creation",
                        ["override2"] = null,
                    },
                    Command = new[] { "printenv" }
                };
                container = await client.Containers.CreateAsync("ddnt:inspect-me-1", options);

                _ = cli.Invoke("container start " + container.Id);
                var output = cli.Invoke("container logs " + container.Id);
                output.Should().Contain("abc=123");
                output.Should().Contain("override1=from-creation");
                output.Should().NotContain("override2");
            }
            finally
            {
                if (container is not null)
                    cli.Invoke("container rm --force " + container.Id, ignoreErrors: true);
            }
        }

        [Fact]
        public async Task CreateAsync_WithPullNever_NoSuchImage_ThrowsImageNotFound()
        {
            using var cli = new DockerCli(toh);
            using var client = await DockerClient.StartAsync();

            IContainer? container = null;
            try
            {
                await Assert.ThrowsAsync<ImageNotFoundLocallyException>(
                    async () => container = await client.Containers.CreateAsync("ddnt:no-such-image-exists-with-this-name"));
            }
            finally
            {
                if (container is not null)
                    cli.Invoke("container rm --force " + container.Id, ignoreErrors: true);
            }
        }

        [Fact]
        public async Task CreateAsync_WithPullIfMissing_NoSuchImage_Pulls()
        {
            using var cli = new DockerCli(toh);
            using var client = await DockerClient.StartAsync();

            cli.Invoke("image rm emdot/dockersdk:empty --force", ignoreErrors: true);

            IContainer? container = null;
            try
            {
                var options = new CreateContainerOptions { PullCondition = PullImageCondition.IfMissing };
                container = await client.Containers.CreateAsync("emdot/dockersdk:empty", options);

                var result = cli.Invoke("image ls emdot/dockersdk:empty --quiet");
                result.Length.Should().Be(1);
            }
            finally
            {
                if (container is not null)
                    cli.Invoke("container rm --force " + container.Id, ignoreErrors: true);
                cli.Invoke("image rm emdot/dockersdk:empty --force", ignoreErrors: true);
            }
        }

        [Fact]
        public async Task StartAsync_AlreadyStarted_NoError()
        {
            using var cli = new DockerCli(toh);
            using var client = await DockerClient.StartAsync();

            var id = cli.Invoke("run --detach ddnt:infinite-loop")[0];
            try
            {
                await client.Containers.StartAsync(id);
            }
            finally
            {
                cli.Invoke("container rm --force " + id, ignoreErrors: true);
            }
        }

        [Fact]
        public async Task StartAsync_DoesNotResolveUntilDone()
        {
            using var cli = new DockerCli(toh);
            using var client = await DockerClient.StartAsync();

            var id = cli.Invoke($"container create ddnt:infinite-loop")[0];
            Container? container = null;
            try
            {
                bool found = false;
                using var subscription = client.Containers.OfType<ContainerStartedEvent>().Where(e => e.ContainerId == id).Subscribe(e => found = true);

                await client.Containers.StartAsync(id);
                Assert.True(found);
            }
            finally
            {
                if (container is not null)
                    cli.Invoke($"container rm --force {id}", ignoreErrors: true);
            }
        }
    }
}
