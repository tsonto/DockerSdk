using System.Runtime.Serialization;

namespace DockerSdk.Core.Models
{
    [DataContract]
    public class SpreadOver // (swarm.SpreadOver)
    {
        [DataMember(Name = "SpreadDescriptor", EmitDefaultValue = false)]
        public string SpreadDescriptor { get; set; }
    }
}
