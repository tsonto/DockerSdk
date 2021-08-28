using System.Runtime.Serialization;

namespace DockerSdk.Core.Models
{
    [DataContract]
    public class DispatcherConfig // (swarm.DispatcherConfig)
    {
        [DataMember(Name = "HeartbeatPeriod", EmitDefaultValue = false)]
        public long HeartbeatPeriod { get; set; }
    }
}
