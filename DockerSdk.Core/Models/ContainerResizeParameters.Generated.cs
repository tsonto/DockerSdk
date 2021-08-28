using System.Runtime.Serialization;

namespace DockerSdk.Core.Models
{
    [DataContract]
    public class ContainerResizeParameters // (main.ContainerResizeParameters)
    {
        [QueryStringParameter("h", false)]
        public long? Height { get; set; }

        [QueryStringParameter("w", false)]
        public long? Width { get; set; }
    }
}
