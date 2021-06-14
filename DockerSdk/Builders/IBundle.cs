using System.IO;
using System.Threading.Tasks;

namespace DockerSdk.Builders
{
    /// <summary>
    /// Encapsulates the data context needed for Docker build methods.
    /// </summary>
    public interface IBundle
    {
        /// <summary>
        /// Gets the relative path to the Dockerfile, from the archive's root.
        /// </summary>
        public string DockerfilePath { get; }

        /// <summary>
        /// Opens a readable stream of a TAR archive that contains the context's files.
        /// </summary>
        /// <returns>A readable stream.</returns>
        public Task<Stream> OpenTarForReadAsync();
    }
}
