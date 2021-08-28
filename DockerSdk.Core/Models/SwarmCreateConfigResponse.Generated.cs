using System.Runtime.Serialization;

namespace DockerSdk.Core.Models
{
    [DataContract]
    public class SwarmCreateConfigResponse // (main.SwarmCreateConfigResponse)
    {
        [DataMember(Name = "ID", EmitDefaultValue = false)]
        public string ID { get; set; }
    }
}
