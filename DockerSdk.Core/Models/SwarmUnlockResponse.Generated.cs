using System.Runtime.Serialization;

namespace DockerSdk.Core.Models
{
    [DataContract]
    public class SwarmUnlockResponse // (main.SwarmUnlockResponse)
    {
        [DataMember(Name = "UnlockKey", EmitDefaultValue = false)]
        public string UnlockKey { get; set; }
    }
}
