using System.Runtime.Serialization;

namespace DockerSdk.Core.Models
{
    [DataContract]
    public class BlkioStatEntry // (types.BlkioStatEntry)
    {
        [DataMember(Name = "major", EmitDefaultValue = false)]
        public ulong Major { get; set; }

        [DataMember(Name = "minor", EmitDefaultValue = false)]
        public ulong Minor { get; set; }

        [DataMember(Name = "op", EmitDefaultValue = false)]
        public string Op { get; set; }

        [DataMember(Name = "value", EmitDefaultValue = false)]
        public ulong Value { get; set; }
    }
}
