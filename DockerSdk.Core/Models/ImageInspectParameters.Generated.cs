using System.Runtime.Serialization;

namespace DockerSdk.Core.Models
{
    [DataContract]
    public class ImageInspectParameters // (main.ImageInspectParameters)
    {
        [QueryStringParameter("size", false)]
        public bool? IncludeSize { get; set; }
    }
}
