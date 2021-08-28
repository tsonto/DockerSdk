using System.Runtime.Serialization;

namespace DockerSdk.Core.Models
{
    [DataContract]
    public class ContainerInspectParameters // (main.ContainerInspectParameters)
    {
        [QueryStringParameter("size", false)]
        public bool? IncludeSize { get; set; }
    }
}
