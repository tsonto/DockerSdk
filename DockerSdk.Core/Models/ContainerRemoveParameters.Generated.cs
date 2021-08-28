using System.Runtime.Serialization;

namespace DockerSdk.Core.Models
{
    [DataContract]
    public class ContainerRemoveParameters // (main.ContainerRemoveParameters)
    {
        [QueryStringParameter("v", false)]
        public bool? RemoveVolumes { get; set; }

        [QueryStringParameter("link", false)]
        public bool? RemoveLinks { get; set; }

        [QueryStringParameter("force", false)]
        public bool? Force { get; set; }
    }
}
