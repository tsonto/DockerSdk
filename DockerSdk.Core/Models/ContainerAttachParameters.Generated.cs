using System.Runtime.Serialization;

namespace DockerSdk.Core.Models
{
    [DataContract]
    public class ContainerAttachParameters
    {
        [QueryStringParameter("stream", false)]
        public bool? Stream { get; set; }

        [QueryStringParameter("stdin", false)]
        public bool? Stdin { get; set; }

        [QueryStringParameter("stdout", false)]
        public bool? Stdout { get; set; }

        [QueryStringParameter("stderr", false)]
        public bool? Stderr { get; set; }

        [QueryStringParameter("detachKeys", false)]
        public string DetachKeys { get; set; }

        [QueryStringParameter("logs", false)]
        public string Logs { get; set; }
    }
}
