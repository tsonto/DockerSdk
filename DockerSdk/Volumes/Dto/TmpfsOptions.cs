using System.Runtime.Serialization;

namespace DockerSdk.Volumes.Dto
{
    internal class TmpfsOptions
    {
        public long? SizeBytes { get; set; }

        public uint? Mode { get; set; }
    }
}
