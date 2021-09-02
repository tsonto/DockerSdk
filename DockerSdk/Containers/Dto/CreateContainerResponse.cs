using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DockerSdk.Containers.Dto
{
    internal class CreateContainerResponse
    {
        public string Id { get; set; } = null!;

        public string[] Warnings { get; set; } = null!;
    }
}
