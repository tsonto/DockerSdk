using System.Runtime.Serialization;

namespace DockerSdk.Core.Models
{
    [DataContract]
    public class PluginConfigNetwork // (types.PluginConfigNetwork)
    {
        [DataMember(Name = "Type", EmitDefaultValue = false)]
        public string Type { get; set; }
    }
}
