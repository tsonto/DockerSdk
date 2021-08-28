using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DockerSdk.Core.Models
{
    [DataContract]
    public class ImagesSearchParameters // (main.ImagesSearchParameters)
    {
        [QueryStringParameter("term", false)]
        public string Term { get; set; }

        [QueryStringParameter("limit", false)]
        public long? Limit { get; set; }

        [QueryStringParameter("filters", false)]
        public IDictionary<string, IDictionary<string, bool>> Filters { get; set; }

        [DataMember(Name = "RegistryAuth", EmitDefaultValue = false)]
        public AuthConfig RegistryAuth { get; set; }
    }
}
