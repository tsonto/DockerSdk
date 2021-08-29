using System.Collections.Generic;
using System.Net;
using DockerSdk.Containers;
using FluentAssertions;
using Xunit;
using CoreBinding = DockerSdk.Core.Models.PortBinding;

namespace DockerSdk.Tests
{
    public class PortBindingUnitTests
    {
        [Fact]
        public void MakeBindings_ComplexCase_GivesCorrectAnswer()
        {
            var input = new PortBinding[]
            {
                new(80, TransportType.Tcp, IPAddress.Parse("1.2.3.0"), 8080),
                new(80, TransportType.Tcp, IPAddress.Parse("1.2.3.1"), 8081),
                new(80, TransportType.Udp, IPAddress.Parse("1.2.3.2"), 8082),
                new(443, TransportType.All, IPAddress.Parse("1.2.3.0"), 443),
            };

            var actual = ContainerAccess.MakePortBindings(input);

            var expected = new Dictionary<string, IList<CoreBinding>>
            {
                ["80/tcp"] = new CoreBinding[]
                {
                    new() { HostIP = "1.2.3.0", HostPort = "8080" },
                    new() { HostIP = "1.2.3.1", HostPort = "8081" },
                },
                ["80/udp"] = new CoreBinding[]
                {
                    new() { HostIP = "1.2.3.2", HostPort = "8082" },
                },
                ["443"] = new CoreBinding[]
                {
                    new() { HostIP = "1.2.3.0", HostPort="443" },
                }
            };

            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void SingleString_AllThreeParts_ParsesCorrectly()
        {
            var actual = new PortBinding("80:8080/tcp");

            actual.ContainerPort.Should().Be(80);
            actual.HostEndpoint.Address.Should().Be(IPAddress.Loopback);
            actual.HostEndpoint.Port.Should().Be(8080);
            actual.Transport.Should().Be(TransportType.Tcp);
        }

        [Fact]
        public void SingleString_FirstPartOnly_ParsesCorrectly()
        {
            var actual = new PortBinding("80");

            actual.ContainerPort.Should().Be(80);
            actual.HostEndpoint.Address.Should().Be(IPAddress.Loopback);
            actual.HostEndpoint.Port.Should().Be(80);
            actual.Transport.Should().Be(TransportType.All);
        }

        [Fact]
        public void SingleString_NoTransport_ParsesCorrectly()
        {
            var actual = new PortBinding("80:8080");

            actual.ContainerPort.Should().Be(80);
            actual.HostEndpoint.Address.Should().Be(IPAddress.Loopback);
            actual.HostEndpoint.Port.Should().Be(8080);
            actual.Transport.Should().Be(TransportType.All);
        }

        [Fact]
        public void TwoString_AllParts_ParsesCorrectly()
        {
            var actual = new PortBinding("80/tcp", "1.2.3.4:8080");

            actual.ContainerPort.Should().Be(80);
            actual.HostEndpoint.Address.ToString().Should().Be("1.2.3.4");
            actual.HostEndpoint.Port.Should().Be(8080);
            actual.Transport.Should().Be(TransportType.Tcp);
        }

        [Fact]
        public void TwoString_WithoutHostPort_ParsesCorrectly()
        {
            var actual = new PortBinding("80/udp", "1.2.3.4");

            actual.ContainerPort.Should().Be(80);
            actual.HostEndpoint.Address.ToString().Should().Be("1.2.3.4");
            actual.HostEndpoint.Port.Should().Be(80);
            actual.Transport.Should().Be(TransportType.Udp);
        }

        [Fact]
        public void TwoString_WithoutIPAddress_ParsesCorrectly()
        {
            var actual = new PortBinding("80/udp", "8080");

            actual.ContainerPort.Should().Be(80);
            actual.HostEndpoint.Address.Should().Be(IPAddress.Loopback);
            actual.HostEndpoint.Port.Should().Be(8080);
            actual.Transport.Should().Be(TransportType.Udp);
        }

        [Fact]
        public void TwoString_WithoutTransport_ParsesCorrectly()
        {
            var actual = new PortBinding("80", "1.2.3.4:8080");

            actual.ContainerPort.Should().Be(80);
            actual.HostEndpoint.Address.ToString().Should().Be("1.2.3.4");
            actual.HostEndpoint.Port.Should().Be(8080);
            actual.Transport.Should().Be(TransportType.All);
        }
    }
}
