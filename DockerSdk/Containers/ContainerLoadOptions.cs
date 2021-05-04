using DockerSdk.Images;
using DockerSdk.Networks;

namespace DockerSdk.Containers
{
    public class ContainerLoadOptions
    {
        public static ContainerLoadOptions Minimal => new() { IncludeDetails = false };
        public static ContainerLoadOptions Shallow => new();

        public bool IncludeDetails { get; set; } = true;

        public NetworkLoadOptions NetworkLoadOptions { get; set; } = NetworkLoadOptions.Minimal;

        public ImageLoadOptions ImageLoadOptions { get; set; } = ImageLoadOptions.Minimal;
    }
}
