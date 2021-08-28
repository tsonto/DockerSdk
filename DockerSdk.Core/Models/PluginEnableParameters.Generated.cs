using System.Runtime.Serialization;

namespace DockerSdk.Core.Models
{
    [DataContract]
    public class PluginEnableParameters // (main.PluginEnableParameters)
    {
        [QueryStringParameter("timeout", false)]
        public long? Timeout { get; set; }
    }
}
