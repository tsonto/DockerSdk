using System;
using System.Linq;
using System.Threading.Tasks;
using DockerSdk.Images;
using DockerSdk.Registries;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace DockerSdk.Tests
{
    [Collection("Common")]
    public class ImageAccessTests
    {
        public ImageAccessTests(ITestOutputHelper toh)
        {
            this.toh = toh;
        }

        private readonly ITestOutputHelper toh;

        [Fact]
        public async Task GetAsync_ByFullId_ImageExists_Success()
        {
            using var client = await DockerClient.StartAsync();
            using var cli = new DockerCli(toh);
            string id = cli.GetImageId("ddnt:inspect-me-1");

            IImage result = await client.Images.GetAsync(id);

            result.Id.ToString().Should().Be(id);
        }

        [Fact]
        public async Task GetAsync_ByName_ImageExists_Success()
        {
            using var client = await DockerClient.StartAsync();
            using var cli = new DockerCli(toh);
            string id = cli.GetImageId("ddnt:inspect-me-1");

            IImage result = await client.Images.GetAsync("ddnt:inspect-me-1");

            result.Id.ToString().Should().Be(id);
        }

        [Fact]
        public async Task GetAsync_ByShortId_ImageExists_Success()
        {
            using var client = await DockerClient.StartAsync();
            using var cli = new DockerCli(toh);
            string id = cli.GetImageId("ddnt:inspect-me-1");

            IImage result = await client.Images.GetAsync(ImageId.Shorten(id));

            result.Id.Should().Equals(id);
        }

        [Fact]
        public async Task GetAsync_ImageDoesNotExist_ThrowsImageNotFoundException()
        {
            using var client = await DockerClient.StartAsync();
            
            await Assert.ThrowsAsync<ImageNotFoundLocallyException>(
                () => client.Images.GetAsync("ddnt:no-such-image-exists-with-this-name"));
        }

        [Fact]
        public async Task GetDetailsAsync_ImageExists_HasCorrectData()
        {
            using var client = await DockerClient.StartAsync();
            using var cli = new DockerCli(toh);
            string id = cli.GetImageId("ddnt:inspect-me-1");

            IImageInfo actual = await client.Images.GetDetailsAsync("ddnt:inspect-me-1");

            actual.Author.Should().Be("3241034+Emdot@users.noreply.github.com");
            actual.Comment.Should().Contain("buildkit");  // BuildKit sets this
            actual.CreationTime.Should().BeBefore(DateTimeOffset.UtcNow);
            actual.CreationTime.Should().BeAfter(DateTimeOffset.Parse("2021-1-1"));
            actual.Digest.Should().BeNull();   // because the image has not been synced
            actual.Id.ToString().Should().Be(id);
            actual.Labels["sample-label-a"].Should().Be("alpha");
            actual.Labels["sample-label-b"].Should().Be("beta");
            actual.ParentImage.Should().BeNull();   // because the image is built by Buildkit
            actual.Size.Should().BeGreaterThan(1_000_000);
            actual.Tags.Select(tag => tag.ToString()).Should().Contain("ddnt:inspect-me-1");
            actual.VirtualSize.Should().BeGreaterThan(1_000_000);
            actual.WorkingDirectory.Should().Be("/sample");
        }

        [Fact]
        public async Task GetDetailsAsync_BuildLabelsOverrideDockerfileLabels()
        {
            using var client = await DockerClient.StartAsync();
            using var cli = new DockerCli(toh);
            string id = cli.GetImageId("ddnt:inspect-me-1");

            IImageInfo actual = await client.Images.GetDetailsAsync("ddnt:inspect-me-1");

            actual.Labels["override-1"].Should().Be("from-build");
        }

        [Fact]
        public async Task GetDetailsAsync_RunLabelsDoNotAffectImageLabels()
        {
            using var client = await DockerClient.StartAsync();
            using var cli = new DockerCli(toh);
            string id = cli.GetImageId("ddnt:inspect-me-1");

            IImageInfo actual = await client.Images.GetDetailsAsync("ddnt:inspect-me-1");

            actual.Labels["override-2"].Should().Be("from-image");
        }

        [Fact]
        public async Task ListAsync_Default_IncludesSampleImages()
        {
            using var client = await DockerClient.StartAsync();
            using var cli = new DockerCli(toh);
            string id1 = cli.GetImageId("ddnt:inspect-me-1");
            string id2 = cli.GetImageId("ddnt:inspect-me-2");

            var list = await client.Images.ListAsync();

            list.Select(image => image.Id.ToString()).Should().Contain(id1, id2);
        }

        [Fact]
        public async Task ListAsync_FilterByLabelExists_Filters()
        {
            using var client = await DockerClient.StartAsync();
            using var cli = new DockerCli(toh);
            string id1 = cli.GetImageId("ddnt:inspect-me-1");

            var options = new ListImagesOptions
            {
                HideIntermediateImages = true,
                DanglingImagesFilter = false,
                LabelExistsFilters = { "sample-label-b" }
            };
            var list = await client.Images.ListAsync(options);

            list.Count.Should().Be(1);
            list.Single().Id.ToString().Should().Be(id1);
        }

        [Fact]
        public async Task ListAsync_FilterByLabelValues_Filters()
        {
            using var client = await DockerClient.StartAsync();
            using var cli = new DockerCli(toh);
            string id1 = cli.GetImageId("ddnt:inspect-me-1");

            var options = new ListImagesOptions
            {
                HideIntermediateImages = true,
                DanglingImagesFilter = false,
                LabelValueFilters = { ["sample-label-b"] = "beta" }
            };
            var list = await client.Images.ListAsync(options);

            list.Count.Should().Be(1);
            list.Single().Id.ToString().Should().Be(id1);
        }

        [Fact]
        public async Task ListAsync_FilterByLabelValues_LabelValueHasEqualSign_Filters()
        {
            using var client = await DockerClient.StartAsync();
            using var cli = new DockerCli(toh);
            string id1 = cli.GetImageId("ddnt:inspect-me-1");

            var options = new ListImagesOptions
            {
                HideIntermediateImages = true,
                DanglingImagesFilter = false,
                LabelValueFilters = { ["label-with-equals"] = "abc=def" }
            };
            var list = await client.Images.ListAsync(options);

            list.Count.Should().Be(1);
            list.Single().Id.ToString().Should().Be(id1);
        }

        [Fact]
        public async Task ListAsync_FilterByReference_Filters()
        {
            using var client = await DockerClient.StartAsync();
            using var cli = new DockerCli(toh);
            string id1 = cli.GetImageId("ddnt:inspect-me-1");
            string id2 = cli.GetImageId("ddnt:inspect-me-2");

            var options = new ListImagesOptions
            {
                ReferencePatternFilters = { "ddnt:inspect-me-?" },
            };
            var list = await client.Images.ListAsync(options);

            list.Count.Should().Be(2);
            list.Select(image => image.Id.ToString()).Should().Contain(id1, id2);
        }

        [Fact]
        public async Task ListAsync_MultipleLabelFilters_UsesAndLogic()
        {
            using var client = await DockerClient.StartAsync();
            using var cli = new DockerCli(toh);

            var options = new ListImagesOptions
            {
                LabelExistsFilters = { "sample-label-b", "sample-label-g" }
            };
            var list = await client.Images.ListAsync(options);

            list.Count.Should().Be(0);
        }

        [Fact]
        public async Task PullAsync_FromPrivateRegistry_WithoutCredentials_ThrowsRegistryAuthException()
        {
            using var client = await DockerClient.StartAsync();
            using var cli = new DockerCli(toh);

            await Assert.ThrowsAsync<RegistryAuthException>(
                () => client.Images.PullAsync("emdot/dockersdk-private:inspect-me-1"));
        }

        [Fact]
        public async Task PullAsync_IsAvailableLocally_RetrievesImage()
        {
            using var client = await DockerClient.StartAsync();
            using var cli = new DockerCli(toh);
            var id = cli.Pull("emdot/dockersdk:empty");

            var image = await client.Images.PullAsync("emdot/dockersdk:empty");

            image.Should().NotBeNull();
            image.Id.ToString().Should().Be(id);
        }

        [Fact]
        public async Task PullAsync_NoSuchImageExists_ThrowsImageNotFoundException()
        {
            using var client = await DockerClient.StartAsync();
            using var cli = new DockerCli(toh);

            await Assert.ThrowsAsync<ImageNotFoundRemotelyException>(
                () => client.Images.PullAsync("emdot/dockersdk:no-such-image-exists-with-this-name"));
        }

        [Fact]
        public async Task PullAsync_NotAvailableLocally_RetrievesImage()
        {
            using var client = await DockerClient.StartAsync();
            using var cli = new DockerCli(toh);
            try
            {
                var image = await client.Images.PullAsync("emdot/dockersdk:empty");

                image.Should().NotBeNull();
            }
            finally
            {
                cli.RemoveImageIfPresent("emdot/dockersdk:empty");
            }
        }
    }
}
