using System.Runtime.Serialization;

namespace DockerSdk.Volumes.Dto
{
    internal class Mount
    {
        public string? Type { get; set; }

        public string? Source { get; set; }

        public string? Target { get; set; }

        public bool? ReadOnly { get; set; }

        public string? Consistency { get; set; }

        public BindOptions? BindOptions { get; set; }

        public VolumeOptions? VolumeOptions { get; set; }

        public TmpfsOptions? TmpfsOptions { get; set; }
    }
}
