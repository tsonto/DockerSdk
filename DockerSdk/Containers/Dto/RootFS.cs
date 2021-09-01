using System.Collections.Generic;

namespace DockerSdk.Containers.Dto
{
    internal class RootFS
    {
        public string Type { get; set; } = null!;

        public IList<string>? Layers { get; set; }

        public string? BaseLayer { get; set; }
    }
}
