using System.Runtime.Serialization;

namespace DockerSdk.Core.Models
{
    [DataContract]
    public class NodeRemoveParameters
    {
        [QueryStringParameter("force", false)]
        public bool Force { get; set; }
    }
}