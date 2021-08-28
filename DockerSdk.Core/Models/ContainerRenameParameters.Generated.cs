using System.Runtime.Serialization;

namespace DockerSdk.Core.Models
{
    [DataContract]
    public class ContainerRenameParameters // (main.ContainerRenameParameters)
    {
        [QueryStringParameter("name", false)]
        public string NewName { get; set; }
    }
}
