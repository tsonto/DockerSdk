using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DockerSdk.Volumes.Dto
{
    internal class VolumeOptions
    {
        public bool? NoCopy { get; set; }

        public IDictionary<string, string>? Labels { get; set; }

        public Driver? DriverConfig { get; set; }
    }
}
