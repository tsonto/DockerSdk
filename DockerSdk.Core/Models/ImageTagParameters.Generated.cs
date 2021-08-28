using System.Runtime.Serialization;

namespace DockerSdk.Core.Models
{
    [DataContract]
    public class ImageTagParameters // (main.ImageTagParameters)
    {
        [QueryStringParameter("repo", false)]
        public string RepositoryName { get; set; }

        [QueryStringParameter("tag", false)]
        public string Tag { get; set; }

        [QueryStringParameter("force", false)]
        public bool? Force { get; set; }
    }
}
