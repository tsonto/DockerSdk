using DockerSdk.Images;
using FluentAssertions;
using Xunit;

namespace DockerSdk.Tests
{
    public class ImageReferenceParsingUnitTests
    {
        [Fact]
        public void Parse_Invalid_Fails()
        {
            ImageReferenceParser.TryParse(":#$%", out _).Should().BeFalse();
        }

        [Fact]
        public void Parse_FullId_ValuesAreCorrect()
        {
            ImageReferenceParser.TryParse("sha256:16afe05194b3482b5429f8630be3a29ed18ca1f102d0b1c720573382b9c46bcf", out var actual)
                .Should().BeTrue();
            actual!.IsFullId.Should().BeTrue();
            actual.LongId.Should().Be("sha256:16afe05194b3482b5429f8630be3a29ed18ca1f102d0b1c720573382b9c46bcf");
            actual.ShortId.Should().Be("16afe05194b3");
        }

        [Fact]
        public void Parse_MediumId_ValuesAreCorrect()
        {
            ImageReferenceParser.TryParse("16afe05194b3482b5429f8630be3a29ed18ca1f102d0b1c720573382b9c46bcf", out var actual)
                .Should().BeTrue();
            actual!.IsFullId.Should().BeTrue();
            actual.IsId.Should().BeTrue();
            actual.LongId.Should().Be("sha256:16afe05194b3482b5429f8630be3a29ed18ca1f102d0b1c720573382b9c46bcf");
            actual.ShortId.Should().Be("16afe05194b3");
        }

        [Fact]
        public void Parse_ShortId_ValuesAreCorrect()
        {
            ImageReferenceParser.TryParse("16afe05194b3", out var actual)
                .Should().BeTrue();
            actual!.IsFullId.Should().BeFalse();
            actual.IsId.Should().BeTrue();
            actual.LongId.Should().BeNull();
            actual.ShortId.Should().Be("16afe05194b3");
        }

        [Fact]
        public void Parse_SimplestName_ValuesAreCorrect()
        {
            ImageReferenceParser.TryParse("test", out var actual)
                .Should().BeTrue();
            actual!.IsName.Should().BeTrue();
            actual.Repository.Should().Be("test");
            actual.Tag.Should().BeNull();
            actual.Digest.Should().BeNull();
            actual.Host.Should().BeNull();
        }

        [Fact]
        public void Parse_AmbiguousComponentWithoutDot_ConsideredNormal()
        {
            ImageReferenceParser.TryParse("test/abc", out var actual)
                .Should().BeTrue();
            actual!.Host.Should().BeNull();
            actual.Repository.Should().Be("test/abc");
        }

        [Fact]
        public void Parse_AmbiguousComponentWithDotWithoutUnderscore_ConsideredHost()
        {
            ImageReferenceParser.TryParse("test.test/abc", out var actual)
                .Should().BeTrue();
            actual!.Host.Should().Be("test.test");
            actual.RepositoryWithoutHost.Should().Be("abc");
        }

        [Fact]
        public void Parse_AmbiguousComponentWithDotWithUnderscore_ConsideredNormal()
        {
            ImageReferenceParser.TryParse("test_test.test/abc", out var actual)
                .Should().BeTrue();
            actual!.Host.Should().BeNull();
            actual.Repository.Should().Be("test_test.test/abc");
        }

        [Fact]
        public void Parse_FirstComponentHasUppercase_ConsideredHost()
        {
            ImageReferenceParser.TryParse("Test/abc", out var actual)
                .Should().BeTrue();
            actual!.Host.Should().Be("Test");
            actual.RepositoryWithoutHost.Should().Be("abc");
        }

        [Fact]
        public void Parse_FirstComponentHasPort_ConsideredHost()
        {
            ImageReferenceParser.TryParse("test:123/abc", out var actual)
                .Should().BeTrue();
            actual!.Host.Should().Be("test");
            actual.Port.Should().Be(123);
            actual.HostWithPort.Should().Be("test:123");
            actual.RepositoryWithoutHost.Should().Be("abc");
        }

        [Fact]
        public void Parse_WithTagWithoutDigest_GetsTag()
        {
            ImageReferenceParser.TryParse("abc:123", out var actual)
                .Should().BeTrue();
            actual!.Host.Should().BeNull();
            actual.Repository.Should().Be("abc");
            actual.Tag.Should().Be("123");
        }

        [Fact]
        public void Parse_WithTagAndDigest_GetsDigestAndTag()
        {
            ImageReferenceParser.TryParse("abc:123@sha256:16afe05194b3482b5429f8630be3a29ed18ca1f102d0b1c720573382b9c46bcf", out var actual)
                .Should().BeTrue();
            actual!.Host.Should().BeNull();
            actual.Repository.Should().Be("abc");
            actual.Tag.Should().Be("123");
            actual.Digest.Should().Be("sha256:16afe05194b3482b5429f8630be3a29ed18ca1f102d0b1c720573382b9c46bcf");
        }

        [Fact]
        public void ParseName_WithTagAndDigest_GetsDigestButTagIsBlank()
        {
            var actual = ImageName.Parse("abc:123@sha256:16afe05194b3482b5429f8630be3a29ed18ca1f102d0b1c720573382b9c46bcf");
            actual.Repository.Should().Be("abc");
            actual.Tag.Should().BeNull();
            actual.Digest.Should().Be("sha256:16afe05194b3482b5429f8630be3a29ed18ca1f102d0b1c720573382b9c46bcf");
        }

        [Fact]
        public void ParseFullId_ShortId_Fails()
        {
            Assert.Throws<MalformedReferenceException>(() => _ = ImageFullId.Parse("16afe05194b3"));
        }

        [Fact]
        public void ParseId_ShortId_Succeeds()
        {
            string actual = ImageId.Parse("16afe05194b3");
            actual.Should().Be("16afe05194b3");
        }

        [Fact]
        public void ParseId_FullId_GivesFullId()
        {
            ImageId actual = ImageId.Parse("sha256:16afe05194b3482b5429f8630be3a29ed18ca1f102d0b1c720573382b9c46bcf");
            actual.Should().BeOfType<ImageFullId>();
            ((string)actual).Should().Be("sha256:16afe05194b3482b5429f8630be3a29ed18ca1f102d0b1c720573382b9c46bcf");
        }

        [Fact]
        public void ParseFullId_FullId_Succeeds()
        {
            string actual = ImageFullId.Parse("sha256:16afe05194b3482b5429f8630be3a29ed18ca1f102d0b1c720573382b9c46bcf");
            actual.Should().Be("sha256:16afe05194b3482b5429f8630be3a29ed18ca1f102d0b1c720573382b9c46bcf");
        }

        [Fact]
        public void Parse_WithoutTagWithDigest_GetsDigest()
        {
            ImageReferenceParser.TryParse("abc@sha256:16afe05194b3482b5429f8630be3a29ed18ca1f102d0b1c720573382b9c46bcf", out var actual)
                .Should().BeTrue();
            actual!.Host.Should().BeNull();
            actual.Repository.Should().Be("abc");
            actual.Tag.Should().BeNull();
            actual.Digest.Should().Be("sha256:16afe05194b3482b5429f8630be3a29ed18ca1f102d0b1c720573382b9c46bcf");
        }

        [Fact]
        public void Parse_FullId_AsId_Succeeds()
        {
            string actual = ImageId.Parse("sha256:16afe05194b3482b5429f8630be3a29ed18ca1f102d0b1c720573382b9c46bcf");
            Assert.Equal("sha256:16afe05194b3482b5429f8630be3a29ed18ca1f102d0b1c720573382b9c46bcf", actual);
        }

        [Fact]
        public void Parse_MediumId_AsId_Succeeds()
        {
            string actual = ImageId.Parse("16afe05194b3482b5429f8630be3a29ed18ca1f102d0b1c720573382b9c46bcf");
            Assert.Equal("sha256:16afe05194b3482b5429f8630be3a29ed18ca1f102d0b1c720573382b9c46bcf", actual);
        }
    }
}
