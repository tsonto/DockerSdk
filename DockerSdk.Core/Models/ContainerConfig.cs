using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using DockerSdk.Core.JsonConverters;

namespace DockerSdk.Core.Models
{
    public class ContainerConfig 
    {
        public string? Hostname { get; set; }

        public string? Domainname { get; set; }

        public string? User { get; set; }

        public bool? AttachStdin { get; set; }

        public bool? AttachStdout { get; set; }

        public bool? AttachStderr { get; set; }

        [JsonConverter(typeof(DictionaryOfEmptyStructsConverter))]
        public IList<string>? ExposedPorts { get; set; }

        public bool? Tty { get; set; }

        public bool? OpenStdin { get; set; }

        public bool? StdinOnce { get; set; }

        public IList<string>? Env { get; set; }

        public IList<string>? Cmd { get; set; }

        public HealthConfig? Healthcheck { get; set; }

        public bool? ArgsEscaped { get; set; }

        public string? Image { get; set; }

        [JsonConverter(typeof(DictionaryOfEmptyStructsConverter))]
        public IList<string>? Volumes { get; set; }

        public string? WorkingDir { get; set; }

        public IList<string>? Entrypoint { get; set; }

        public bool? NetworkDisabled { get; set; }

        public string? MacAddress { get; set; }

        public IList<string>? OnBuild { get; set; }

        public IDictionary<string, string>? Labels { get; set; }

        public string? StopSignal { get; set; }

        [JsonConverter(typeof(TimeSpanSecondsConverter))]
        public TimeSpan? StopTimeout { get; set; }

        public IList<string>? Shell { get; set; }
    }
}
