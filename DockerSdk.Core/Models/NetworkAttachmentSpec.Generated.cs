using System.Runtime.Serialization;

namespace DockerSdk.Core.Models
{
    [DataContract]
    public class NetworkAttachmentSpec // (swarm.NetworkAttachmentSpec)
    {
        [DataMember(Name = "ContainerID", EmitDefaultValue = false)]
        public string ContainerID { get; set; }
    }
}
