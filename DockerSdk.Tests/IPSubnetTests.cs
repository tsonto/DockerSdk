using System.Net;
using DockerSdk.Networks;
using FluentAssertions;
using Xunit;

namespace DockerSdk.Tests
{
    public class IPSubnetTests
    {
        [Theory]
        [InlineData("12.34.56.78/8", "12.0.0.0/8")]
        [InlineData("1234:0678:0000::9abc:def0/27", "1234:660::/27")]
        [InlineData("1234:0678:0000::9abc:def0/0", "::/0")]
        [InlineData("127.65530/8", "127.0.0.0/8")]    // old-style class A network address
        public void Stringify(string input, string expected)
        {
            var subnet = IPSubnet.Parse(input);
            subnet.ToString().Should().Be(expected);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("and/or")]
        [InlineData("300.300.300.300/24")]
        [InlineData("12.34.56.78/33")]
        [InlineData("12.34.56.78/")]
        [InlineData("12.34.56.78/-1")]
        [InlineData("::/129")]
        [InlineData("/24")]
        public void TryParse_NotWellFormed(string? input)
        {
            IPSubnet.TryParse(input, out _, out _).Should().BeFalse();
        }

        [Theory]
        [InlineData("12.34.56.78/32", "12.34.56.78", 32, "12.34.56.78")]
        [InlineData("12.34.56.78/24", "12.34.56.00", 24, "12.34.56.78")]
        [InlineData("12.34.56.78/20", "12.34.48.00", 20, "12.34.56.78")]
        [InlineData("12.34.56.78/19", "12.34.32.00", 19, "12.34.56.78")]
        [InlineData("12.34.56.78/8", "12.00.00.00", 8, "12.34.56.78")]
        [InlineData("12.34.56.78/0", "0.0.0.0", 0, "12.34.56.78")]
        [InlineData("1234:5678::9abc:def0/128", "1234:5678::9abc:def0", 128, "1234:5678::9abc:def0")]
        [InlineData("1234:5678::9abc:def0/16", "1234::", 16, "1234:5678::9abc:def0")]
        [InlineData("1234:5678::9abc:def0/27", "1234:5660::", 27, "1234:5678::9abc:def0")]
        [InlineData("1234:5678::9abc:def0/0", "::", 0, "1234:5678::9abc:def0")]
        [InlineData("127.65530/8", "127.0.0.0", 8, "127.0.255.250")]    // old-style class A network address
        public void TryParse_WellFormed(string input, string expectedBaseStr, int expectedLength, string expectedGivenStr)
        {
            var actualSuccess = IPSubnet.TryParse(input, out var actualSubnet, out var actualGiven);

            actualSuccess.Should().BeTrue();
            actualSubnet!.Base.Should().Be(IPAddress.Parse(expectedBaseStr));
            actualSubnet.PrefixLength.Should().Be(expectedLength);
            actualGiven!.Should().Be(IPAddress.Parse(expectedGivenStr));
        }
    }
}
