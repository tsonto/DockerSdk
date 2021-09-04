using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DockerSdk.Daemon
{
    internal class VersionResponse
    {
        /// <summary>
        /// The default (and highest) API version that is supported by the daemon
        /// </summary>
        public string? ApiVersion { get; set; }

        /// <summary>
        /// The architecture that the daemon is running on
        /// </summary>
        [JsonPropertyName("Arch")]
        public string? Architecture { get; set; }

        /// <summary>
        /// The date and time that the daemon was compiled.
        /// </summary>
        public string? BuildTime { get; set; }

        /// <summary>
        /// Information about system components
        /// </summary>
        public IList<ComponentVersion>? Components { get; set; }

        /// <summary>
        /// Indicates whether the daemon was started with experimental features enabled.
        /// </summary>
        public bool? Experimental { get; set; }

        /// <summary>
        /// The Git commit of the source code that was used to build the daemon
        /// </summary>
        public string? GitCommit { get; set; }

        /// <summary>
        /// The version Go used to compile the daemon, and the version of the Go runtime in use.
        /// </summary>
        public string? GoVersion { get; set; }

        /// <summary>
        /// The kernel version (<c>uname -r</c>) that the daemon is running on.
        /// </summary>
        public string? KernelVersion { get; set; }

        /// <summary>
        /// The minimum API version that is supported by the daemon
        /// </summary>
        [JsonPropertyName("MinAPIVersion")]
        public string MinimumApiVersion { get; set; } = null!;

        /// <summary>
        /// The operating system that the daemon is running on ("linux" or "windows")
        /// </summary>
        [JsonPropertyName("Os")]
        public string? OS { get; set; }

        /// <summary>
        /// The version of the daemon
        /// </summary>
        public string? Version { get; set; }
    }
}
