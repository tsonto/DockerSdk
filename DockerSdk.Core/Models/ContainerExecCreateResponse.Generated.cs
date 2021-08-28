using System.Runtime.Serialization;

namespace DockerSdk.Core.Models
{
    [DataContract]
    public class ContainerExecCreateResponse // (main.ContainerExecCreateResponse)
    {
        [DataMember(Name = "Id", EmitDefaultValue = false)]
        public string ID { get; set; }
    }
}
