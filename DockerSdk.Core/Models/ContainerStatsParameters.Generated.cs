using System.Runtime.Serialization;

namespace DockerSdk.Core.Models
{
    [DataContract]
    public class ContainerStatsParameters // (main.ContainerStatsParameters)
    {
        [QueryStringParameter("stream", true)]
        public bool Stream { get; set; } = true;
    }
}
