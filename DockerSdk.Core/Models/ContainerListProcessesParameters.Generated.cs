using System.Runtime.Serialization;

namespace DockerSdk.Core.Models
{
    [DataContract]
    public class ContainerListProcessesParameters // (main.ContainerListProcessesParameters)
    {
        [QueryStringParameter("ps_args", false)]
        public string PsArgs { get; set; }
    }
}
