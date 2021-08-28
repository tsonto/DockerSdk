using System.Runtime.Serialization;

namespace DockerSdk.Core.Models
{
    [DataContract]
    public class EncryptionConfig // (swarm.EncryptionConfig)
    {
        [DataMember(Name = "AutoLockManagers", EmitDefaultValue = false)]
        public bool AutoLockManagers { get; set; }
    }
}
