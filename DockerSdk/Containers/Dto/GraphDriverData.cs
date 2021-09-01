using System.Collections.Generic;

namespace DockerSdk.Containers.Dto
{
    internal class GraphDriverData
    {
        public IDictionary<string, string>? Data { get; set; }

        public string Name { get; set; } = null!;
    }
}
