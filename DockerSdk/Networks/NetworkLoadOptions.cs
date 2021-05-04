using DockerSdk.Containers;

namespace DockerSdk.Networks
{
    public class NetworkLoadOptions
    {
        public static NetworkLoadOptions Minimal => new() { IncludeDetails = false };

        public static NetworkLoadOptions Shallow => new();

        public ContainerLoadOptions ContainerLoadOptions { get; set; } = ContainerLoadOptions.Minimal;
        public bool IncludeDetails { get; set; } = true;
    }
}
