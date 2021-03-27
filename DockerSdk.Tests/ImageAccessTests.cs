using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DockerSdk.Images;
using FluentAssertions;
using Xunit;

namespace DockerSdk.Tests
{
    [Collection("Common")]
    public class ImageAccessTests
    {
        private const string SampleImageName = "ddnt:inspect-me-1";
        private const string NonImageName = "ddnt:no-such-image-exists-with-this-name";

        private static string ReadSampleImageId()
        {
            var output = Cli.Run("docker images ls --no-trunc --quiet --filter reference=" + SampleImageName);
            return output[0];
        }

        [Fact]
        public async Task GetAsync_ByName_ImageExists_Success()
        {
            using var client = await DockerClient.StartAsync();

            Image result = await client.Images.GetAsync(SampleImageName);

            string id = ReadSampleImageId();
            result.Id.Should().Be(id);
        }

        [Fact]
        public async Task GetAsync_ByFullId_ImageExists_Success()
        {
            using var client = await DockerClient.StartAsync();

            string id = ReadSampleImageId();
            Image result = await client.Images.GetAsync(id);

            result.Id.Should().Be(id);
        }

        [Fact]
        public async Task GetAsync_ByShortId_ImageExists_Success()
        {
            using var client = await DockerClient.StartAsync();

            string id = ReadSampleImageId();
            Image result = await client.Images.GetAsync(ImageAccess.ShortenId(id));

            result.Id.Should().Equals(id);
        }

        [Fact]
        public async Task GetAsync_ImageDoesNotExist_ThrowsImageNotFoundException()
        {
            using var client = await DockerClient.StartAsync();

            await Assert.ThrowsAsync<ImageNotFoundException>(
                () => client.Images.GetAsync(NonImageName));
        }

        [Fact]
        public async Task GetDetailsAsync_ImageExists_HasCorrectData()
        {
            using var client = await DockerClient.StartAsync();

            ImageDetails actual = await client.Images.GetDetailsAsync(SampleImageName);

            var id = ReadSampleImageId();
            actual.Author.Should().Be("3241034+Emdot@users.noreply.github.com");
            actual.Comment.Should().Be("");   // how to set this?
            actual.CreationTime.Should().BeBefore(DateTimeOffset.UtcNow);
            actual.CreationTime.Should().BeAfter(DateTimeOffset.UtcNow.AddDays(-1));
            actual.Digest.Should().BeNull();   // because the image has not been synced
            actual.Id.Should().Be(id);
            actual.IdShort.Should().Be(ImageAccess.ShortenId(id));
            actual.Labels["sample-label-a"].Should().Be("alpha");
            actual.Labels["sample-label-b"].Should().Be("beta");
            actual.ParentImage.Should().NotBeNull();
            actual.Size.Should().Be(5_613_130);
            actual.Tags.Should().Contain(SampleImageName);
            actual.VirtualSize.Should().Be(5_613_130);
            actual.WorkingDirectory.Should().Be("/sample");
        }
    }
}
