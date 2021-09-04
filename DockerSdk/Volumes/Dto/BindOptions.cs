using System.Runtime.Serialization;

namespace DockerSdk.Volumes.Dto
{
    internal class BindOptions
    {
        public string? Propagation { get; set; }

        public bool? NonRecursive { get; set; }
    }
}
