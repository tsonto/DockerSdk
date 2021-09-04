namespace DockerSdk.Volumes.Dto
{
    internal class ListResponse
    {
        /// <summary>
        /// List of volumes.
        /// </summary>
        public VolumeResponse[] Volumes { get; set; } = null!;

        /// <summary>
        /// Warnings that occurred when fetching the list of volumes.
        /// </summary>
        public string[] Warnings { get; set; } = null!;
    }
}
