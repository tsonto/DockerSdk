using System.Runtime.Serialization;

namespace DockerSdk.Core.Models
{
    [DataContract]
    public class ContainerPathStatParameters // (main.ContainerPathStatParameters)
    {
        [QueryStringParameter("path", true)]
        public string Path { get; set; }

        [QueryStringParameter("noOverwriteDirNonDir", false)]
        public bool? AllowOverwriteDirWithFile { get; set; }
    }
}
