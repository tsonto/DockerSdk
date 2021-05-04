using DockerSdk.Containers;

namespace DockerSdk.Images
{
    public class ImageLoadOptions
    {
        public static ImageLoadOptions Minimal => new() { IncludeDetails = false };
        public static ImageLoadOptions Shallow => new();

        public ContainerLoadOptions ChildContainerLoadOptions { get; set; } = ContainerLoadOptions.Minimal;
        public bool IncludeDetails { get; set; } = true;

        public ImageLoadOptions ParentImageLoadOptions { get; set; } = ImageLoadOptions.Minimal;
    }
}
