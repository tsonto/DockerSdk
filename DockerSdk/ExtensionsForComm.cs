using System.Net.Http;
using DockerSdk.Core;

namespace DockerSdk
{
    public static class ExtensionsForComm
    {
        public static RequestBuilder Build(this Comm comm, HttpMethod method, string path) => new RequestBuilder(comm, method, path);
    }
}
