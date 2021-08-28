using System.Runtime.Serialization;

namespace DockerSdk.Core.Models
{
    [DataContract]
    public class ImageDeleteParameters // (main.ImageDeleteParameters)
    {
        [QueryStringParameter("force", false)]
        public bool? Force { get; set; }

        [QueryStringParameter("noprune", false)]
        public bool? NoPrune { get; set; }
    }
}
