using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DockerSdk.Core.Models
{
    [DataContract]
    public class NetworksDeleteUnusedParameters // (main.NetworksDeleteUnusedParameters)
    {
        [QueryStringParameter("filters", false)]
        public IDictionary<string, IDictionary<string, bool>> Filters { get; set; }
    }
}