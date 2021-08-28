using System.Runtime.Serialization;

namespace DockerSdk.Core.Models
{
    [DataContract]
    public class OrchestrationConfig // (swarm.OrchestrationConfig)
    {
        [DataMember(Name = "TaskHistoryRetentionLimit", EmitDefaultValue = false)]
        public long? TaskHistoryRetentionLimit { get; set; }
    }
}
