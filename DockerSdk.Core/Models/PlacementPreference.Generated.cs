using System.Runtime.Serialization;

namespace DockerSdk.Core.Models
{
    [DataContract]
    public class PlacementPreference // (swarm.PlacementPreference)
    {
        [DataMember(Name = "Spread", EmitDefaultValue = false)]
        public SpreadOver Spread { get; set; }
    }
}
