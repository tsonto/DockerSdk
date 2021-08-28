using System.Runtime.Serialization;

namespace DockerSdk.Core.Models
{
    [DataContract]
    public class ConfigReference // (network.ConfigReference)
    {
        [DataMember(Name = "Network", EmitDefaultValue = false)]
        public string Network { get; set; }
    }
}
