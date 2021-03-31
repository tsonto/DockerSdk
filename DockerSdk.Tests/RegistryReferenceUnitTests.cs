using DockerSdk.Registries;
using Xunit;

namespace DockerSdk.Tests
{
    public class RegistryReferenceUnitTests
    {
        [Theory]
        [InlineData("abc", "abc")]
        [InlineData("example.com", "example.com")]
        [InlineData("Example.com", "example.com")]
        [InlineData("abc:123", "abc:123")]
        [InlineData("abc_d", null)]
        [InlineData("abc:d", null)]
        [InlineData("Abc", "abc")]
        [InlineData("abc:0", null)]
        [InlineData("abc:123456", null)]
        public void TryParse_Tests(string input, string? expected)
        {
            _ = RegistryReference.TryParse(input, out RegistryReference? actual);
            Assert.Equal(expected, actual?.ToString());
        }
    }
}
