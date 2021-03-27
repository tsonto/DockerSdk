using System;
using DockerSdk.Images;
using Xunit;

namespace DockerSdk.Tests
{
    public class ImageAccessUnitTests
    {
        [Theory]
        [InlineData("sha256:678dfa38fcfa349ccbdb1b6d52ac113ace67d5746794b36dfbad9dd96a9d1c43", "678dfa38fcfa")]
        [InlineData("678dfa38fcfa349ccbdb1b6d52ac113ace67d5746794b36dfbad9dd96a9d1c43", "678dfa38fcfa")]
        [InlineData("678dfa38fcfa", "678dfa38fcfa")]
        public void ShortenId_SuccessfulCases(string input, string expected)
        {
            var actual = ImageAccess.ShortenId(input);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("678dfa38fcfa349ccbdb1b6d")]
        [InlineData("mcr.microsoft.com/dotnet/runtime")]
        [InlineData("mcr.microsoft.com/dotnet/runtime:5.0-buster-slim")]
        public void ShortenId_ExceptionCases(string input)
        {
            Assert.Throws<ArgumentException>(
                () => _ = ImageAccess.ShortenId(input));
        }
    }
}
