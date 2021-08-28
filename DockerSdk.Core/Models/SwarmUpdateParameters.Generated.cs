using System.Runtime.Serialization;

namespace DockerSdk.Core.Models
{
    [DataContract]
    public class SwarmUpdateParameters // (main.SwarmUpdateParameters)
    {
        [DataMember(Name = "Spec", EmitDefaultValue = false)]
        public Spec Spec { get; set; }

        [QueryStringParameter("version", true)]
        public long Version { get; set; }

        [QueryStringParameter("rotateworkertoken", false)]
        public bool? RotateWorkerToken { get; set; }

        [QueryStringParameter("rotatemanagertoken", false)]
        public bool? RotateManagerToken { get; set; }

        [QueryStringParameter("rotatemanagerunlockkey", false)]
        public bool? RotateManagerUnlockKey { get; set; }
    }
}
