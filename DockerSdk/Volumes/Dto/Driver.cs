using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DockerSdk.Volumes.Dto
{
    internal class Driver
    {
        public string Name { get; set; } = null!;

        public IDictionary<string, string>? Options { get; set; }
    }
}
