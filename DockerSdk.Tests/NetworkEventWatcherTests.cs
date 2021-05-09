using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Threading.Tasks;
using DockerSdk.Containers;
using DockerSdk.Networks;
using DockerSdk.Networks.Events;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace DockerSdk.Tests
{
    [Collection("Common")]
    public class NetworkEventWatcherTests
    {
        public NetworkEventWatcherTests(ITestOutputHelper toh)
        {
            this.toh = toh;
        }

        private readonly ITestOutputHelper toh;

        [Fact]
        public async Task LifecycleTest()
        {
            using var cli = new DockerCli(toh);
            using var client = await DockerClient.StartAsync();
            var bridgeId = cli.GetBridgeNetworkId();

            IContainer? container = null;
            INetwork? network = null;
            try
            {
                var events = new List<NetworkEvent>();
                using var subscription = client.Networks.Subscribe(e => events.Add(e));

                // `Invoke` calls return when the CLI command has run, but the Docker operation may take a bit longer to
                // complete. This local method waits until either the expected number of events have arrived or a
                // certain amount of time has elapsed.
                var maxWait = TimeSpan.FromSeconds(5);
                async Task LogCountAsync(int count)
                {
                    Stopwatch sw = Stopwatch.StartNew();
                    while (events!.Count < count)
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(100));
                        if (sw.Elapsed > maxWait)
                            return;
                    }
                }

                container = cli.CreateAndStartContainer(client);
                await LogCountAsync(1);
                events.Should().HaveCount(1);
                events[0].NetworkId.ToString().Should().Be(bridgeId);
                events[0].EventType.Should().Be(NetworkEventType.Attached);
                events[0].Should().BeOfType<NetworkAttachedEvent>();
                (events[0] as NetworkAttachedEvent)!.ContainerId.Should().Be(container.Id);

                network = cli.CreateNetwork(client);
                await LogCountAsync(2);
                events.Should().HaveCount(2);
                events[1].NetworkId.Should().Be(network.Id);
                events[1].EventType.Should().Be(NetworkEventType.Created);
                events[1].Should().BeOfType<NetworkCreatedEvent>();

                _ = cli.Invoke($"network connect {network.Id} {container.Id}");
                await LogCountAsync(3);
                events.Should().HaveCount(3);
                events[2].NetworkId.Should().Be(network.Id);
                events[2].EventType.Should().Be(NetworkEventType.Attached);
                events[2].Should().BeOfType<NetworkAttachedEvent>();
                (events[2] as NetworkAttachedEvent)!.ContainerId.Should().Be(container.Id);

                _ = cli.Invoke($"network disconnect {network.Id} {container.Id}");
                await LogCountAsync(4);
                events.Should().HaveCountGreaterOrEqualTo(4);
                events[3].NetworkId.Should().Be(network.Id);
                events[3].EventType.Should().Be(NetworkEventType.Detached);
                events[3].Should().BeOfType<NetworkDetachedEvent>();
                (events[3] as NetworkDetachedEvent)!.ContainerId.Should().Be(container.Id);

                var rmIndex = 4;
                _ = cli.Invoke($"network rm {network.Id}");
                await LogCountAsync(5);
                if (events.Count >= 5 && events[4].EventType == NetworkEventType.Detached)
                {
                    // We got two detach events. This is a quirk of Docker, which we just ignore.
                    rmIndex = 5;
                    await LogCountAsync(6);
                }
                events.Should().HaveCount(rmIndex + 1);
                events[rmIndex].NetworkId.Should().Be(network.Id);
                events[rmIndex].EventType.Should().Be(NetworkEventType.Deleted);
                events[rmIndex].Should().BeOfType<NetworkDeletedEvent>();
            }
            finally
            {
                cli.CleanUpContainer(container);
                cli.CleanUpNetwork(network);
            }
        }

        [Fact]
        public async Task SubscribeAndUnsubscribe_DoesNotThrow()
        {
            using var client = await DockerClient.StartAsync();

            using var subscription = client.Networks.Subscribe(e => { });
        }
    }
}
