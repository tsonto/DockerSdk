using System.Runtime.Serialization;

namespace DockerSdk.Core.Models
{
    [DataContract]
    public class ServiceLogsParameters // (main.ServiceLogsParameters)
    {
        [QueryStringParameter("stdout", false)]
        public bool? ShowStdout { get; set; }

        [QueryStringParameter("stderr", false)]
        public bool? ShowStderr { get; set; }

        [QueryStringParameter("since", false)]
        public string Since { get; set; }

        [QueryStringParameter("timestamps", false)]
        public bool? Timestamps { get; set; }

        [QueryStringParameter("follow", false)]
        public bool? Follow { get; set; }

        [QueryStringParameter("tail", false)]
        public string Tail { get; set; }

        [QueryStringParameter("details", false)]
        public bool? Details { get; set; }
    }
}
