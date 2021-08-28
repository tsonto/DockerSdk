using System.Runtime.Serialization;

namespace DockerSdk.Core.Models
{
    [DataContract]
    public class ImageLoadParameters // (main.ImageLoadParameters)
    {
        [QueryStringParameter("quiet", true)]
        public bool Quiet { get; set; }
    }
}
