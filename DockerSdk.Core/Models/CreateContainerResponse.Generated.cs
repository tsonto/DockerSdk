using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DockerSdk.Core.Models
{
    [DataContract]
    public class CreateContainerResponse // (container.ContainerCreateCreatedBody)
    {
        [DataMember(Name = "Id", EmitDefaultValue = false)]
        public string ID { get; set; }

        [DataMember(Name = "Warnings", EmitDefaultValue = false)]
        public IList<string> Warnings { get; set; }
    }
}