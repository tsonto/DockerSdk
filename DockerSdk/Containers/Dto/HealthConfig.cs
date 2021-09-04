using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using DockerSdk.JsonConverters;

namespace DockerSdk.Containers.Dto
{
    internal class HealthConfig
    {
        [JsonConverter(typeof(TimeSpanNanosecondsConverter))]
        public TimeSpan? Interval { get; set; }

        public long? Retries { get; set; }
        public long? StartPeriod { get; set; }
        public IList<string>? Test { get; set; }

        [JsonConverter(typeof(TimeSpanSecondsConverter))]
        public TimeSpan? Timeout { get; set; }
    }
}
