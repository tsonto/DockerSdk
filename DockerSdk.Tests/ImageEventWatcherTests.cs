using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reactive.Linq;
using System.Threading.Tasks;
using DockerSdk.Images;
using DockerSdk.Images.Events;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace DockerSdk.Tests
{
    [Collection("Common")]
    public class ImageEventWatcherTests
    {
        public ImageEventWatcherTests(ITestOutputHelper toh)
        {
            this.toh = toh;
        }

        private readonly ITestOutputHelper toh;

        [Fact]
        public async Task LifecycleTest_TagUntagSaveLoad()
        {
            using var cli = new DockerCli(toh);
            using var client = await DockerClient.StartAsync();
            var imageName = ImageName.Parse("ddnt:inspect-me-1");
            var imageAlias = ImageName.Parse("ddnt:temp");
            var tempPath = Path.GetTempFileName();

            IImage image = cli.GetImage(client, imageName);
            try
            {
                var events = new List<ImageEvent>();
                using var subscription = client.Images.Subscribe(e => events.Add(e));

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

                _ = cli.Invoke($"image tag {image.Id} {imageAlias}");
                await LogCountAsync(1);
                events.Should().HaveCount(1);
                events[0].ImageReference.Should().Be(image.Id);
                events[0].EventType.Should().Be(ImageEventType.Tagged);
                events[0].Should().BeOfType<ImageTaggedEvent>();
                (events[0] as ImageTaggedEvent)!.ImageId.Should().Be(image.Id);
                (events[0] as ImageTaggedEvent)!.ImageName.Should().Be(imageAlias);

                _ = cli.Invoke($"image save --output {tempPath} {imageAlias}");
                await LogCountAsync(2);
                events.Should().HaveCount(2);
                events[1].ImageReference.ToString().Should().Be(image.Id.ToString());
                events[1].EventType.Should().Be(ImageEventType.Saved);
                events[1].Should().BeOfType<ImageSavedEvent>();

                _ = cli.Invoke($"image rm {imageAlias}");
                await LogCountAsync(3);
                events.Should().HaveCount(3);
                events[2].ImageReference.Should().Be(image.Id);
                events[2].EventType.Should().Be(ImageEventType.Untagged);
                events[2].Should().BeOfType<ImageUntaggedEvent>();
                (events[2] as ImageUntaggedEvent)!.ImageId.Should().Be(image.Id);

                _ = cli.Invoke($"image load --input {tempPath}");
                await LogCountAsync(4);
                events.Should().HaveCount(4);
                events[3].ImageReference.ToString().Should().Be(image.Id.ToString());
                events[3].EventType.Should().Be(ImageEventType.Loaded);
                events[3].Should().BeOfType<ImageLoadedEvent>();
            }
            finally
            {
                _ = cli.Invoke($"image untag {imageAlias}", ignoreErrors: true);
                File.Delete(tempPath);
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
