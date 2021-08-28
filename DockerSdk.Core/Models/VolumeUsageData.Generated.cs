using System.Runtime.Serialization;

namespace DockerSdk.Core.Models
{
    [DataContract]
    public class VolumeUsageData // (types.VolumeUsageData)
    {
        [DataMember(Name = "RefCount", EmitDefaultValue = false)]
        public long RefCount { get; set; }

        [DataMember(Name = "Size", EmitDefaultValue = false)]
        public long Size { get; set; }
    }
}
