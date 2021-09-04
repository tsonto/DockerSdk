using System.Collections.Generic;

namespace DockerSdk.Containers.Dto
{
    internal class LogConfig
    {
        public string Type { get; set; } = null!;

        public IDictionary<string, string>? Config { get; set; }
    }
}
