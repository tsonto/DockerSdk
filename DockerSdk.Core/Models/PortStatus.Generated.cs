using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DockerSdk.Core.Models
{
    [DataContract]
    public class PortStatus // (swarm.PortStatus)
    {
        [DataMember(Name = "Ports", EmitDefaultValue = false)]
        public IList<PortConfig> Ports { get; set; }
    }
}
