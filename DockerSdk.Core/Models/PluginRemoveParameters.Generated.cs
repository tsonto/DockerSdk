using System.Runtime.Serialization;

namespace DockerSdk.Core.Models
{
    [DataContract]
    public class PluginRemoveParameters // (main.PluginRemoveParameters)
    {
        [QueryStringParameter("force", false)]
        public bool? Force { get; set; }
    }
}
