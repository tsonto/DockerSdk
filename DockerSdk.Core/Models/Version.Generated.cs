using System.Runtime.Serialization;

namespace DockerSdk.Core.Models
{
    [DataContract]
    public class Version // (swarm.Version)
    {
        [DataMember(Name = "Index", EmitDefaultValue = false)]
        public ulong Index { get; set; }
    }
}
