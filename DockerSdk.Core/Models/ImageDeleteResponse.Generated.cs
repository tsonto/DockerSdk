using System.Runtime.Serialization;

namespace DockerSdk.Core.Models
{
    [DataContract]
    public class ImageDeleteResponse // (types.ImageDeleteResponseItem)
    {
        [DataMember(Name = "Deleted", EmitDefaultValue = false)]
        public string Deleted { get; set; }

        [DataMember(Name = "Untagged", EmitDefaultValue = false)]
        public string Untagged { get; set; }
    }
}
