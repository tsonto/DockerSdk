using System.Collections.Generic;

namespace DockerSdk.Daemon
{
    internal class ComponentVersion
    {
        public IDictionary<string, string>? Details { get; set; }
        public string? Name { get; set; }

        public string? Version { get; set; }
    }
}
