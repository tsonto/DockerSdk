using System.Runtime.Serialization;

namespace DockerSdk.Core.Models
{
    [DataContract]
    public class Address // (network.Address)
    {
        [DataMember(Name = "Addr", EmitDefaultValue = false)]
        public string Addr { get; set; }

        [DataMember(Name = "PrefixLen", EmitDefaultValue = false)]
        public long PrefixLen { get; set; }
    }
}
