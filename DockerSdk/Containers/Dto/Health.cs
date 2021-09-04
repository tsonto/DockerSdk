using System.Collections.Generic;

namespace DockerSdk.Containers.Dto
{
    internal class Health
    {
        public long FailingStreak { get; set; }
        public IList<HealthcheckResult>? Log { get; set; }
        public string? Status { get; set; }
    }
}
