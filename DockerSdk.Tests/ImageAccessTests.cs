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
        private const string LocalImage1Name = "ddnt:inspect-me-1";
        private const string LocalImage2Name = "ddnt:inspect-me-2";
        private const string LocalNonImageName = "ddnt:no-such-image-exists-with-this-name";
        private const string RemoteImage1Name = "emdot/docker-dotnet-test:empty.1";
        private const string RemoteNonImageName = "emdot/docker-dotnet-test:no-such-image-exists-with-this-name";
        private const string PrivateImage1Name = "emdot/dockersdk-private:inspect-me-1";

        private static string GetImageId(string imageName)
        {
            var output = Cli.Run("docker images ls --no-trunc --quiet --filter reference=" + imageName);
            return output[0];
        }

        [Fact]
        public async Task GetAsync_ByName_ImageExists_Success()
        {
            using var client = await DockerClient.StartAsync();

            Image result = await client.Images.GetAsync(LocalImage1Name);

            string id = GetImageId(LocalImage1Name);
            result.Id.ToString().Should().Be(id);
        }

        [Fact]
        public async Task GetAsync_ByFullId_ImageExists_Success()
        {
            using var client = await DockerClient.StartAsync();

            string id = GetImageId(LocalImage1Name);
            Image result = await client.Images.GetAsync(id);

            result.Id.ToString().Should().Be(id);
        }

        [Fact]
        public async Task GetAsync_ByShortId_ImageExists_Success()
        {
            using var client = await DockerClient.StartAsync();

            string id = GetImageId(LocalImage1Name);
            Image result = await client.Images.GetAsync(ImageId.Shorten(id));

            result.Id.Should().Equals(id);
        }

        [Fact]
        public async Task GetAsync_ImageDoesNotExist_ThrowsImageNotFoundException()
        {
            using var client = await DockerClient.StartAsync();

            await Assert.ThrowsAsync<ImageNotFoundException>(
                () => client.Images.GetAsync(LocalNonImageName));
        }

        [Fact]
        public async Task GetDetailsAsync_ImageExists_HasCorrectData()
        {
            using var client = await DockerClient.StartAsync();

            ImageDetails actual = await client.Images.GetDetailsAsync(LocalImage1Name);

            var id = GetImageId(LocalImage1Name);
            actual.Author.Should().Be("3241034+Emdot@users.noreply.github.com");
            actual.Comment.Should().Be("");   // how to set this?
            actual.CreationTime.Should().BeBefore(DateTimeOffset.UtcNow);
            actual.CreationTime.Should().BeAfter(DateTimeOffset.Parse("2021-1-1"));
            actual.Digest.Should().BeNull();   // because the image has not been synced
            actual.Id.ToString().Should().Be(id);
            actual.Labels["sample-label-a"].Should().Be("alpha");
            actual.Labels["sample-label-b"].Should().Be("beta");
            actual.ParentImage.Should().NotBeNull();
            actual.Size.Should().Be(5_613_130);
            actual.Tags.Select(tag => tag.ToString()).Should().Contain(LocalImage1Name);
            actual.VirtualSize.Should().Be(5_613_130);
            actual.WorkingDirectory.Should().Be("/sample");
        }

        [Fact]
        public async Task ListAsync_Default_IncludesSampleImages()
        {
            using var client = await DockerClient.StartAsync();

            var list = await client.Images.ListAsync();

            var sample1Id = GetImageId(LocalImage1Name);
            var sample2Id = GetImageId(LocalImage2Name);
            list.Select(image => image.Id.ToString()).Should().Contain(sample1Id, sample2Id);
        }

        [Fact]
        public async Task ListAsync_FilterByReference_Filters()
        {
            using var client = await DockerClient.StartAsync();

            var options = new ListImagesOptions
            {
                ReferencePatternFilters = { "ddnt:inspect-me-?" },
            };
            var list = await client.Images.ListAsync(options);

            var sample1Id = GetImageId(LocalImage1Name);
            var sample2Id = GetImageId(LocalImage2Name);
            list.Count.Should().Be(2);
            list.Select(image => image.Id.ToString()).Should().Contain(sample1Id, sample2Id);
        }

        [Fact]
        public async Task ListAsync_FilterByLabelExists_Filters()
        {
            using var client = await DockerClient.StartAsync();

            var options = new ListImagesOptions
            {
                HideIntermediateImages = true,
                DanglingImagesFilter = false,
                LabelExistsFilters = { "sample-label-b" }
            };
            var list = await client.Images.ListAsync(options);

            var sample1Id = GetImageId(LocalImage1Name);
            list.Count.Should().Be(1);
            list.Single().Id.ToString().Should().Be(sample1Id);
        }

        [Fact]
        public async Task ListAsync_FilterByLabelValues_Filters()
        {
            using var client = await DockerClient.StartAsync();

            var options = new ListImagesOptions
            {
                HideIntermediateImages = true,
                DanglingImagesFilter = false,
                LabelValueFilters = { ["sample-label-b"] = "beta" }
            };
            var list = await client.Images.ListAsync(options);

            var sample1Id = GetImageId(LocalImage1Name);
            list.Count.Should().Be(1);
            list.Single().Id.ToString().Should().Be(sample1Id);
        }

        [Fact]
        public async Task ListAsync_FilterByLabelValues_LabelValueHasEqualSign_Filters()
        {
            using var client = await DockerClient.StartAsync();

            var options = new ListImagesOptions
            {
                HideIntermediateImages = true,
                DanglingImagesFilter = false,
                LabelValueFilters = { ["label-with-equals"] = "abc=def" }
            };
            var list = await client.Images.ListAsync(options);

            var sample1Id = GetImageId(LocalImage1Name);
            list.Count.Should().Be(1);
            list.Single().Id.ToString().Should().Be(sample1Id);
        }

        [Fact]
        public async Task ListAsync_MultipleLabelFilters_UsesAndLogic()
        {
            using var client = await DockerClient.StartAsync();

            var options = new ListImagesOptions
            {
                LabelExistsFilters = { "sample-label-b", "sample-label-g" }
            };
            var list = await client.Images.ListAsync(options);

            var sample1Id = GetImageId(LocalImage1Name);
            var sample2Id = GetImageId(LocalImage1Name);
            list.Count.Should().Be(0);
        }

        [Fact]
        public async Task PullAsync_NotAvailableLocally_RetrievesImage()
        {
            _ = Cli.Run($"docker image rm {RemoteImage1Name}", ignoreErrors: true);

            using var client = await DockerClient.StartAsync();

            var image = await client.Images.PullAsync(RemoteImage1Name);

            image.Should().NotBeNull();

            _ = Cli.Run($"docker image rm {RemoteImage1Name}", ignoreErrors: true);
        }

        [Fact]
        public async Task PullAsync_IsAvailableLocally_RetrievesImage()
        {
            _ = Cli.Run($"docker image pull {RemoteImage1Name}", ignoreErrors: true);

            using var client = await DockerClient.StartAsync();

            var image = await client.Images.PullAsync(RemoteImage1Name);

            image.Should().NotBeNull();

            _ = Cli.Run($"docker image rm {RemoteImage1Name}", ignoreErrors: true);
        }

        [Fact]
        public async Task PullAsync_NoSuchImageExists_ThrowsImageNotFoundException()
        {
            _ = Cli.Run($"docker image rm {RemoteNonImageName}", ignoreErrors: true);

            using var client = await DockerClient.StartAsync();

            await Assert.ThrowsAsync<ImageNotFoundException>(
                () => client.Images.PullAsync(RemoteNonImageName));
        }

        [Fact]
        public async Task PullAsync_FromPrivateRegistry_WithoutCredentials_ThrowsRegistryAuthException()
        {
            _ = Cli.Run($"docker image rm {PrivateImage1Name}", ignoreErrors: true);

            using var client = await DockerClient.StartAsync();

            await Assert.ThrowsAsync<RegistryAuthException>(
                () => client.Images.PullAsync(PrivateImage1Name));
        }
    }
}
