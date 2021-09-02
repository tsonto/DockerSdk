using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace DockerSdk.Containers.Dto
{
    internal class RestartPolicy
    {
        public RestartPolicyKind Name { get; set; }

        public long MaximumRetryCount { get; set; }
    }
}
