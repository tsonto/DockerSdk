using System;

namespace DockerSdk.Containers.Dto
{
    internal class HealthcheckResult
    {
        public DateTimeOffset? End { get; set; }
        public long? ExitCode { get; set; }
        public string? Output { get; set; }
        public DateTimeOffset Start { get; set; }
    }
}
