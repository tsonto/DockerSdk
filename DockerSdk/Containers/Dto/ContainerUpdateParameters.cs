using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace DockerSdk.Containers.Dto
{
    internal class ContainerUpdateParameters
    {
        public long? CpuShares { get; set; }

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
        public long KernelMemoryTcp { get; set; }

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

        public RestartPolicy? RestartPolicy { get; set; }
    }
}
