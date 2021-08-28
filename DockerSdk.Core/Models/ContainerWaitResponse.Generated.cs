using System.Runtime.Serialization;

namespace DockerSdk.Core.Models
{
    [DataContract]
    public class ContainerWaitResponse // (main.ContainerWaitResponse)
    {
        [DataMember(Name = "StatusCode", EmitDefaultValue = false)]
        public long StatusCode { get; set; }
    }
}
