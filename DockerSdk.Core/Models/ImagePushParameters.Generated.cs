using System.Runtime.Serialization;

namespace DockerSdk.Core.Models
{
    [DataContract]
    public class ImagePushParameters // (main.ImagePushParameters)
    {
        [QueryStringParameter("tag", false)]
        public string Tag { get; set; }
    }
}
