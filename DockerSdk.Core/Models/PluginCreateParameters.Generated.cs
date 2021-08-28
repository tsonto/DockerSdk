using System.Runtime.Serialization;

namespace DockerSdk.Core.Models
{
    [DataContract]
    public class PluginCreateParameters // (main.PluginCreateParameters)
    {
        [QueryStringParameter("name", true)]
        public string Name { get; set; }
    }
}
