using System;
using System.Threading.Tasks;
using DockerSdk.Volumes;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace DockerSdk.Tests
{
    [Collection("Common")]
    public class VolumeAccessTests
    {
        public VolumeAccessTests(ITestOutputHelper toh)
        {
            this.toh = toh;
        }

        private readonly ITestOutputHelper toh;

        [Fact]
        public async Task GetDetailsAsync_NoSuchVolume_ThrowsVolumeNotFoundException()
        {
            using var client = await DockerClient.StartAsync();

            await Assert.ThrowsAnyAsync<VolumeNotFoundException>(
                () => client.Volumes.GetDetailsAsync("ddnt-no-such-volume-366ad733152b70e53ddd7fd59defe9fa2e055ed2090f5f3a8839b2797388d0b4"));
        }

        [Fact]
        public async Task GetDetailsAsync_LocalVolumeExists_GetsExpectedDetails()
        {
            using var client = await DockerClient.StartAsync();

            var volume = await client.Volumes.GetDetailsAsync("ddnt-general");

            volume.Should().NotBeNull();
            volume.CreationTime.Should().BeAfter(DateTimeOffset.Parse("2020-1-1"));
            volume.Driver.Should().Be("local");
            volume.Labels["ddnt1"].Should().Be("alef");
            volume.Labels["ddnt2"].Should().Be("beth");
            volume.Mountpoint.Should().NotBeNullOrWhiteSpace();
            volume.Name.ToString().Should().Be("ddnt-general");
            volume.Scope.Should().Be(VolumeScope.Local);
        }

        [Fact]
        public async Task ListAsync_NoFilters_FindsPremadeVolumes()
        {
            using var client = await DockerClient.StartAsync();

            var volumes = await client.Volumes.ListAsync();

            volumes.Should().ContainSingle(v => v.Name.ToString() == "ddnt-general");
        }
    }
}
