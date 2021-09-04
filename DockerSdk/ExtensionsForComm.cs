using System.Net.Http;
using DockerSdk.Core;

namespace DockerSdk
{
    /// <summary>
    /// Provides extension methods for the <see cref="Comm"/> class.
    /// </summary>
    public static class ExtensionsForComm
    {
        /// <summary>
        /// Starts a new builder to construct a request for the Docker daemon.
        /// </summary>
        /// <param name="comm">The Docker communications object that will do the sending and receiving.</param>
        /// <param name="method">The HTTP method (fka verb) of the request.</param>
        /// <param name="path">The URL of the request, relative to the daemon's API base.</param>
        /// <returns>The new builder.</returns>
        public static RequestBuilder Build(this Comm comm, HttpMethod method, string path) => new RequestBuilder(comm, method, path);
    }
}
