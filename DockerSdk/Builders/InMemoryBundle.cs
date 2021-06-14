using System.IO;
using System.Threading.Tasks;

namespace DockerSdk.Builders
{
    internal class InMemoryBundle : IBundle
    {
        public InMemoryBundle(string dockerfilePath, byte[] tar)
        {
            DockerfilePath = dockerfilePath;
            this.tar = tar;
        }

        public string DockerfilePath { get; }
        private readonly byte[] tar;

        public Task<Stream> OpenTarForReadAsync()
            => Task.FromResult<Stream>(new MemoryStream(tar));
    }
}
