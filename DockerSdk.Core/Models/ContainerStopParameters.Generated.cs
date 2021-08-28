using System.Runtime.Serialization;

namespace DockerSdk.Core.Models
{
    [DataContract]
    public class ContainerStopParameters // (main.ContainerStopParameters)
    {
        [QueryStringParameter("t", false)]
        public uint? WaitBeforeKillSeconds { get; set; }
    }
}
