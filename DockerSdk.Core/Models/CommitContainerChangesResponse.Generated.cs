using System.Runtime.Serialization;

namespace DockerSdk.Core.Models
{
    [DataContract]
    public class CommitContainerChangesResponse // (main.CommitContainerChangesResponse)
    {
        [DataMember(Name = "Id", EmitDefaultValue = false)]
        public string ID { get; set; }
    }
}
