using System.Runtime.Serialization;

namespace DockerSdk.Core.Models
{
    [DataContract]
    public class NetworksCreateResponse // (types.NetworkCreateResponse)
    {
        [DataMember(Name = "Id", EmitDefaultValue = false)]
        public string ID { get; set; }

        [DataMember(Name = "Warning", EmitDefaultValue = false)]
        public string Warning { get; set; }
    }
}
