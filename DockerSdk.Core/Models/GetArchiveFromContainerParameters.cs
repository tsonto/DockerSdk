namespace DockerSdk.Core.Models
{
    public class GetArchiveFromContainerParameters
    {
        [QueryStringParameter("path", true)]
        public string? Path { get; set; }
    }
}
