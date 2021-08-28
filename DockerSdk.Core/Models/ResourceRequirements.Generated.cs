using System.Runtime.Serialization;

namespace DockerSdk.Core.Models
{
    [DataContract]
    public class ResourceRequirements // (swarm.ResourceRequirements)
    {
        [DataMember(Name = "Limits", EmitDefaultValue = false)]
        public SwarmLimit Limits { get; set; }

        [DataMember(Name = "Reservations", EmitDefaultValue = false)]
        public SwarmResources Reservations { get; set; }
    }
}
