using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DockerSdk.Core.Models
{
    [DataContract]
    public class ContainersListParameters // (main.ContainersListParameters)
    {
        [QueryStringParameter("size", false)]
        public bool? Size { get; set; }

        [QueryStringParameter("all", false)]
        public bool? All { get; set; }

        [QueryStringParameter("since", false)]
        public string Since { get; set; }

        [QueryStringParameter("before", false)]
        public string Before { get; set; }

        [QueryStringParameter("limit", false)]
        public long? Limit { get; set; }

        [QueryStringParameter("filters", false)]
        public IDictionary<string, IDictionary<string, bool>> Filters { get; set; }
    }
}
