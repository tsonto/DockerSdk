using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using DockerSdk.Volumes.Dto;

namespace DockerSdk.Containers.Dto
{
    internal class HostConfig
    {
        public IList<string>? Binds { get; set; }

        [JsonPropertyName("ContainerIDFile")]
        public string? ContainerIdFile { get; set; }

        public LogConfig? LogConfig { get; set; }

        public string? NetworkMode { get; set; }

        public IDictionary<string, IList<Networks.Dto.PortBinding>>? PortBindings { get; set; }

        public RestartPolicy? RestartPolicy { get; set; }

        public bool? AutoRemove { get; set; }

        public string? VolumeDriver { get; set; }

        public IList<string>? VolumesFrom { get; set; }

        public IList<string>? CapAdd { get; set; }

        public IList<string>? CapDrop { get; set; }

        public string? CgroupnsMode { get; set; }

        public IList<string>? Dns { get; set; }

        public IList<string>? DnsOptions { get; set; }

        public IList<string>? DnsSearch { get; set; }

        public IList<string>? ExtraHosts { get; set; }

        public IList<string>? GroupAdd { get; set; }

        public string? IpcMode { get; set; }

        public string? Cgroup { get; set; }

        public IList<string>? Links { get; set; }

        public long? OomScoreAdj { get; set; }

        public string? PidMode { get; set; }

        public bool? Privileged { get; set; }

        public bool? PublishAllPorts { get; set; }

        public bool? ReadonlyRootfs { get; set; }

        public IList<string>? SecurityOpt { get; set; }

        public IDictionary<string, string>? StorageOpt { get; set; }

        public IDictionary<string, string>? Tmpfs { get; set; }

        [JsonPropertyName("UTSMode")]
        public string? UtsMode { get; set; }

        public string? UsernsMode { get; set; }

        public long? ShmSize { get; set; }

        public IDictionary<string, string>? Sysctls { get; set; }

        public string? Runtime { get; set; }

        public ulong[]? ConsoleSize { get; set; }

        public string? Isolation { get; set; }

        public long? CPUShares { get; set; }

        public long? Memory { get; set; }

        public long? NanoCpus { get; set; }

        public string? CgroupParent { get; set; }

        public ushort? BlkioWeight { get; set; }

        public IList<WeightDevice>? BlkioWeightDevice { get; set; }

        public IList<ThrottleDevice>? BlkioDeviceReadBps { get; set; }

        public IList<ThrottleDevice>? BlkioDeviceWriteBps { get; set; }

        public IList<ThrottleDevice>? BlkioDeviceReadIOps { get; set; }

        public IList<ThrottleDevice>? BlkioDeviceWriteIOps { get; set; }

        public long? CpuPeriod { get; set; }

        public long? CpuQuota { get; set; }

        public long? CpuRealtimePeriod { get; set; }

        public long? CpuRealtimeRuntime { get; set; }

        public string? CpusetCpus { get; set; }

        public string? CpusetMems { get; set; }

        public IList<DeviceMapping>? Devices { get; set; }

        public IList<string>? DeviceCgroupRules { get; set; }

        public IList<DeviceRequest>? DeviceRequests { get; set; }

        public long? KernelMemory { get; set; }

        [JsonPropertyName("KernelMemoryTCP")]
        public long? KernelMemoryTcp { get; set; }

        public long? MemoryReservation { get; set; }

        public long? MemorySwap { get; set; }

        public long? MemorySwappiness { get; set; }

        public bool? OomKillDisable { get; set; }

        public long? PidsLimit { get; set; }

        public IList<Ulimit>? Ulimits { get; set; }

        public long? CpuCount { get; set; }

        public long? CpuPercent { get; set; }

        public ulong? IOMaximumIOps { get; set; }

        public ulong? IOMaximumBandwidth { get; set; }

        public IList<Mount>? Mounts { get; set; }

        public IList<string>? MaskedPaths { get; set; }

        public IList<string>? ReadonlyPaths { get; set; }

        public bool? Init { get; set; }
    }
}
