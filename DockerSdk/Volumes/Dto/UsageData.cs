namespace DockerSdk.Volumes.Dto
{
    /// <summary>
    /// Usage details about the volume. This information is used by the GET /system/df endpoint, and omitted in other
    /// endpoints.
    /// </summary>
    internal class UsageData
    {
        /// <summary>
        /// The number of containers referencing this volume. This field is set to -1 if the reference-count is not
        /// available.
        /// </summary>
        public int RefCount { get; set; } = -1;

        /// <summary>
        /// Amount of disk space used by the volume (in bytes). This information is only available for volumes created
        /// with the "local" volume driver. For volumes created with other volume drivers, this field is set to -1 ("not
        /// available")
        /// </summary>
        public long Size { get; set; } = -1;
    }
}
