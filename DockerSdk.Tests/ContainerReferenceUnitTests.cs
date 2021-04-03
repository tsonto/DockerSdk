using DockerSdk.Containers;
using Xunit;

namespace DockerSdk.Tests
{
    public class ContainerReferenceUnitTests
    {
        [Fact]
        public void TryParse_FullId_Success()
        {
            string value = "366ad733152b70d53ddd7ec59deee9fa2ef55ed2095f5f3a8899b2797388d0b4";
            Assert.True(ContainerReference.TryParse(value, out var reference));
            Assert.IsType<ContainerFullId>(reference);
            Assert.Equal(value, reference!.ToString());
        }

        [Fact]
        public void TryParse_InvalidFullId_Fails()
        {
            string value = "X66ad733152b70d53ddd7ec59deee9fa2ef55ed2095f5f3a8899b2797388d0b4";
            Assert.False(ContainerFullId.TryParse(value, out var reference));
        }

        [Fact]
        public void TryParse_InvalidName_Fail()
        {
            string value = "clever+wozniak";
            Assert.False(ContainerReference.TryParse(value, out var reference));
        }

        [Fact]
        public void TryParse_InvalidShortId_Fail()
        {
            string value = "366ad733152b0";
            Assert.False(ContainerId.TryParse(value, out var reference));
        }

        [Fact]
        public void TryParse_Name_Success()
        {
            string value = "clever_wozniak";
            Assert.True(ContainerReference.TryParse(value, out var reference));
            Assert.IsType<ContainerName>(reference);
            Assert.Equal(value, reference!.ToString());
        }

        [Fact]
        public void TryParse_ShortId_Success()
        {
            string value = "366ad733152b";
            Assert.True(ContainerReference.TryParse(value, out var reference));
            Assert.IsType<ContainerId>(reference);
            Assert.Equal(value, reference!.ToString());
        }

        [Fact]
        public void Shorten_OnLongId_ProducesShortId()
        {
            var actual = new ContainerFullId("366ad733152b70d53ddd7ec59deee9fa2ef55ed2095f5f3a8899b2797388d0b4").ShortForm;
            Assert.Equal("366ad733152b", actual);
        }

        [Fact]
        public void Shorten_OnShortId_SameAsInput()
        {
            var actual = new ContainerFullId("366ad733152b").ShortForm;
            Assert.Equal("366ad733152b", actual);
        }
    }
}
