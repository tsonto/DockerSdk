using DockerSdk.Networks;
using FluentAssertions;
using Xunit;

namespace DockerSdk.Tests
{
    public class NetworkReferenceUnitTests
    {
        [Theory]
        [InlineData("69a3e5d5cc4bcb51b1ee47ea70e09a465d4622174b87fd7b28639dbdf4e35a8d", true)] // local ID, full form
        [InlineData("69A3E5D5CC4BCB51B1EE47EA70E09A465D4622174B87FD7B28639DBDF4E35A8D", false)]
        [InlineData("qqa3e5d5cc4bcb51b1ee47ea70e09a465d4622174b87fd7b28639dbdf4e35a8d", false)]
        [InlineData("69a3e5d5cc4bcb51b1ee47ea70e09a465d4622174b87fd7b28639dbdf4e35a", false)]
        [InlineData("69a3e5d5cc4bcb51b1ee47ea70e09a465d4622174b87fd7b28639dbdf4e35a8dee", false)]
        [InlineData("ytvt45bprzrw4vwsztage5cr6", true)] // swarm ID, full form
        [InlineData("==vt45bprzrw4vwsztage5cr6", false)]
        [InlineData("ytvt45bprzrw4vwsztage5cr66", false)]
        [InlineData("ytvt45bprzrw4vwsztage5cr", false)]
        [InlineData("1fb7872118c7", true)] // typical local ID, short form
        [InlineData("1fb7872118c77", false)]
        [InlineData("1fb7872118c", false)]
        [InlineData("ytvt45bprzrw", true)] // typical swarm ID, short form
        [InlineData("ytvt45bprzrw1", false)]
        [InlineData("ytvt45bprzr", false)]
        public void NetworkId_TryParse(string input, bool expectedReturn)
        {
            var actualReturn = NetworkId.TryParse(input, out NetworkId? actualOut);

            actualReturn.Should().Be(expectedReturn);

            if (expectedReturn)
                actualOut?.ToString().Should().Be(input);
            else
                actualOut.Should().BeNull();
        }
    }
}
