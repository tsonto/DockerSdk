using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DockerSdk.Core.Models
{
    [DataContract]
    public class ImagesListParameters // (main.ImagesListParameters)
    {
        [QueryStringParameter("all", false)]
        public bool? All { get; set; }

        [QueryStringParameter("filters", false)]
        public IDictionary<string, IDictionary<string, bool>> Filters { get; set; }
    }
}
