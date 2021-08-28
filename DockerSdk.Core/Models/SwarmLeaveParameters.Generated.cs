using System.Runtime.Serialization;

namespace DockerSdk.Core.Models
{
    [DataContract]
    public class SwarmLeaveParameters // (main.SwarmLeaveParameters)
    {
        [QueryStringParameter("force", false)]
        public bool? Force { get; set; }
    }
}
