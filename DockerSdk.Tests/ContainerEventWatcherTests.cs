﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Threading.Tasks;
using DockerSdk.Containers;
using DockerSdk.Containers.Events;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace DockerSdk.Tests
{
    [Collection("Common")]
    public class ContainerEventWatcherTests
    {
        public ContainerEventWatcherTests(ITestOutputHelper toh)
        {
            this.toh = toh;
        }

        private readonly ITestOutputHelper toh;

        [Fact]
        public async Task LifecycleTest()
        {
            using var cli = new DockerCli(toh);
            using var client = await DockerClient.StartAsync();
            var name = ContainerName.Parse("ddnt-" + nameof(LifecycleTest));

            var log = new List<ContainerEvent>();
            using var subscription = client.Containers.Subscribe(e => log.Add(e));

            // Invoke calls return when the CLI command has run, but the Docker operation may take a bit longer to
            // complete. This local method waits until either the expected number of log messages have arrived or a
            // certain amount of time has elapsed.
            var maxWait = TimeSpan.FromSeconds(5);
            async Task LogCountAsync(int count)
            {
                Stopwatch sw = Stopwatch.StartNew();
                while (log!.Count < count)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(100));
                    if (sw.Elapsed > maxWait)
                        return;
                }
            }

            var containerId = cli.Invoke($"run --name {name} --detach ddnt:infinite-loop")[0];
            try
            {
                await LogCountAsync(2);
                log.Should().HaveCount(2);
                log[0].ContainerId.ToString().Should().Be(containerId);
                log[0].EventType.Should().Be(ContainerEventType.Created);
                log[0].Should().BeOfType<ContainerCreatedEvent>();
                log[1].ContainerId.ToString().Should().Be(containerId);
                log[1].EventType.Should().Be(ContainerEventType.Started);
                log[1].Should().BeOfType<ContainerStartedEvent>();

                _ = cli.Invoke($"pause {containerId}");
                await LogCountAsync(3);
                log.Should().HaveCount(3);
                log[2].ContainerId.ToString().Should().Be(containerId);
                log[2].EventType.Should().Be(ContainerEventType.Paused);
                log[2].Should().BeOfType<ContainerPausedEvent>();

                _ = cli.Invoke($"unpause {containerId}");
                await LogCountAsync(4);
                log.Should().HaveCount(4);
                log[3].ContainerId.ToString().Should().Be(containerId);
                log[3].EventType.Should().Be(ContainerEventType.Unpaused);
                log[3].Should().BeOfType<ContainerUnpausedEvent>();

                _ = cli.Invoke($"stop {containerId}");
                await LogCountAsync(8);
                log.Should().HaveCount(8);
                log[4].ContainerId.ToString().Should().Be(containerId);
                log[4].EventType.Should().Be(ContainerEventType.Signalled);
                log[4].Should().BeOfType<ContainerSignalledEvent>();
                ((ContainerSignalledEvent)log[4]).SignalNumber.Should().Be(15);
                log[5].ContainerId.ToString().Should().Be(containerId);
                log[5].EventType.Should().Be(ContainerEventType.Signalled);
                log[5].Should().BeOfType<ContainerSignalledEvent>();
                ((ContainerSignalledEvent)log[5]).SignalNumber.Should().Be(9);
                log[6].ContainerId.ToString().Should().Be(containerId);
                log[6].EventType.Should().Be(ContainerEventType.Exited);
                log[6].Should().BeOfType<ContainerExitedEvent>();
                ((ContainerExitedEvent)log[6]).ExitCode.Should().Be(137);
                log[7].ContainerId.ToString().Should().Be(containerId);
                log[7].EventType.Should().Be(ContainerEventType.StopCompleted);
                log[7].Should().BeOfType<ContainerStopCompletedEvent>();

                _ = cli.Invoke($"rm {containerId}");
                await LogCountAsync(9);
                log.Should().HaveCount(9);
                log[8].ContainerId.ToString().Should().Be(containerId);
                log[8].EventType.Should().Be(ContainerEventType.Deleted);
                log[8].Should().BeOfType<ContainerDeletedEvent>();
            }
            finally
            {
                cli.Invoke($"container rm --force {containerId}", ignoreErrors: true);
            }
        }

        [Fact]
        public async Task SubscribeAndUnsubscribe_DoesNotThrow()
        {
            using var client = await DockerClient.StartAsync();

            using var subscription = client.Containers.Subscribe(e => { });
        }
    }
}
