using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace DockerSdk.Core.Models
{
    public class ObjectExtensionData
    {
        [JsonExtensionData]
        public IDictionary<string, object>? ExtensionData { get; set; }
    }
}
