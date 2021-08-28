using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DockerSdk.Core.Models
{
    [DataContract]
    public class PluginConfigureParameters // (main.PluginConfigureParameters)
    {
        [DataMember(Name = "Args", EmitDefaultValue = false)]
        public IList<string> Args { get; set; }
    }
}
