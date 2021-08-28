using System.Runtime.Serialization;

namespace DockerSdk.Core.Models
{
    [DataContract]
    public class PluginDisableParameters // (main.PluginDisableParameters)
    {
        [QueryStringParameter("force", false)]
        public bool? Force { get; set; }
    }
}
