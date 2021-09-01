namespace DockerSdk.Containers.Dto
{
    internal class ThrottleDevice
    {
        public string? Path { get; set; }

        public ulong? Rate { get; set; }
    }
}
