namespace DockerSdk.Containers.Dto
{
    internal class Ulimit
    {
        public string Name { get; set; } = null!;

        public long? Hard { get; set; }

        public long? Soft { get; set; }
    }
}
