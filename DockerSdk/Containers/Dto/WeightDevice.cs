using System.Runtime.Serialization;

namespace DockerSdk.Containers.Dto
{
    internal class WeightDevice
    {
        public string? Path { get; set; }

        public ushort? Weight { get; set; }
    }
}
