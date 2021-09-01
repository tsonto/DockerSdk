using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using DockerSdk.Core.JsonConverters;

namespace DockerSdk.Core.Models
{
    public class CreateContainerParameters
    {
        //public CreateContainerParameters(ContainerConfig Config)
        //{
        //    if (Config != null)
        //    {
        //        this.Hostname = Config.Hostname;
        //        this.Domainname = Config.Domainname;
        //        this.User = Config.User;
        //        this.AttachStdin = Config.AttachStdin;
        //        this.AttachStdout = Config.AttachStdout;
        //        this.AttachStderr = Config.AttachStderr;
        //        this.ExposedPorts = Config.ExposedPorts;
        //        this.Tty = Config.Tty;
        //        this.OpenStdin = Config.OpenStdin;
        //        this.StdinOnce = Config.StdinOnce;
        //        this.Env = Config.Env;
        //        this.Cmd = Config.Cmd;
        //        this.Healthcheck = Config.Healthcheck;
        //        this.ArgsEscaped = Config.ArgsEscaped;
        //        this.Image = Config.Image;
        //        this.Volumes = Config.Volumes;
        //        this.WorkingDir = Config.WorkingDir;
        //        this.Entrypoint = Config.Entrypoint;
        //        this.NetworkDisabled = Config.NetworkDisabled;
        //        this.MacAddress = Config.MacAddress;
        //        this.OnBuild = Config.OnBuild;
        //        this.Labels = Config.Labels;
        //        this.StopSignal = Config.StopSignal;
        //        this.StopTimeout = Config.StopTimeout;
        //        this.Shell = Config.Shell;
        //    }
        //}

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

        public HostConfig? HostConfig { get; set; }

        public NetworkingConfig? NetworkingConfig { get; set; }
    }
}
