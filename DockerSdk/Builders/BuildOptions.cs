using System;
using System.Collections.Generic;
using DockerSdk.Images;

namespace DockerSdk.Builders
{
    /// <summary>
    /// Specifies how to build an image.
    /// </summary>
    public class BuildOptions
    {
        /// <summary>
        /// Gets or sets a collection of tags (names) to apply to the image when it has been created.
        /// </summary>
        public IEnumerable<ImageName> Tags { get; set; } = Array.Empty<ImageName>();

        /// <summary>
        /// Gets or sets which build stage to run.
        /// </summary>
        public string? TargetBuildStage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the daemon should use its build cache when building the image. The
        /// default is true.
        /// </summary>
        /// <remarks>
        /// Disabling the build cache makes builds slower. Typically you would only disable it for troubleshooting
        /// purposes.
        /// </remarks>
        public bool UseBuildCache { get; set; } = true;

        /// <summary>
        /// Gets a collection of the labels to apply to the image.
        /// </summary>
        public Dictionary<string, string> Labels = new();

        /*  // TODO: restore this when https://github.com/dotnet/Docker.DotNet/issues/522 is fixed
        /// <summary>
        /// Gets or sets a value indicating whether the daemon should attempt to re-pull the build's images. Regardless
        /// of this setting, the daemon will always pull build images that it doesn't have at all. The default is false.
        /// </summary>
        /// <remarks>
        /// This option is mostly useful when the Dockerfile referenes tags that may point to different versions at
        /// different times, such as "latest". It has no practical effect on images that the Dockerfile specifies by
        /// digest.
        /// </remarks>
        public bool ForcePull { get; set; }
        */
    }
}
