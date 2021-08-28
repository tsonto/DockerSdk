using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DockerSdk.Core.Models
{
    [DataContract]
    public class ServiceListParameters // (main.ServiceListParameters)
    {
        [QueryStringParameter("filters", false)]
        public IDictionary<string, IDictionary<string, bool>> Filters { get; set; }

        [QueryStringParameter("status", false)]
        public bool? Status { get; set; }
    }
}
